using GSOD_DataProcessor.Models;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System.Text;
using TinyCsvParser;

namespace GSOD_DataProcessor.Business;

public class DataProcessor
{
    public static async Task Start()
    {
        Logging.Log("GSOD Data Processor", "Start");
        if (!NoaaSiteParsing.CheckNoaaSiteIfUpdateAvailable())
        {
            Logging.Log("GSOD Data Processor", "End", true);
            return;
        }

        await GetNewestGSOD();
        DecompressGSOD();
        ParseGSODData();
        GetOutOfDateStations();
        Logging.Log("GSOD Data Processor", "End", true);
    }

    public static async Task GetNewestGSOD()
    {
        Logging.Log("GetNewestGSOD", "Start");
        try
        {
            using var client = new HttpClient();
            using var s = await client.GetStreamAsync(AppSettings.NoaaGsodUri + NoaaArchive.NewestArchiveFileName);
            using var fs = new FileStream(NoaaArchive.NewestArchiveFileName, FileMode.OpenOrCreate);
            await s.CopyToAsync(fs);
            Logging.Log("GetNewestGSOD", "Success");
        }
        catch (Exception ex)
        {
            Logging.Log("GetNewestGSOD", $"Failure - {ex.Message}");
            FailureShutdown();
        }
    }

    public static void DecompressGSOD()
    {
        Logging.Log("DecompressGSOD", "Start");
        try
        {
            Stream inStream = File.OpenRead(NoaaArchive.NewestArchiveFileName);
            Stream gzipStream = new GZipInputStream(inStream);

            TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.ASCII);
            tarArchive.ExtractContents(NoaaArchive.UncompressedFolderName);
            tarArchive.Close();

            gzipStream.Close();
            inStream.Close();
            File.Delete(NoaaArchive.NewestArchiveFileName);
            Logging.Log("DecompressGSOD", "Success");
        }
        catch (Exception ex)
        {
            Logging.Log("DecompressGSOD", $"Failure - {ex.Message}");
            FailureShutdown();
        }
    }

    public static void ParseGSODData()
    {
        Logging.Log("ParseGSODData", "Start");
        try
        {
            CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
            CsvStationGSODMapping csvMapper = new CsvStationGSODMapping();
            CsvParser<StationGSOD> csvParser = new CsvParser<StationGSOD>(csvParserOptions, csvMapper);
            DateOnly startDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-8));

            var files = Directory.EnumerateFiles(NoaaArchive.UncompressedFolderName, "*.csv");

            foreach (var file in files)
            {
                var result = csvParser
                    .ReadFromFile(file, Encoding.ASCII)
                    .Where(x => x.IsValid && DateOnly.FromDateTime(x.Result.Date) > startDate)
                    .Select(x => x.Result)
                    .ToList();

                if (result.Count > 0)
                {
                    StationList.AddToStations(new PastWeekStationData(result));
                }
            }
            Directory.Delete(NoaaArchive.UncompressedFolderName, true);
            Logging.Log("ParseGSODData", "Success");
        }
        catch (Exception ex)
        {
            Logging.Log("ParseGSODData", $"Failure - {ex.Message}");
            FailureShutdown();
        }
    }

    // TODO: MEMORY HOOOOOG
    public static void GetOutOfDateStations()
    {
        Logging.Log("GetOutOfDateStations", "Start");
        try
        {
            var dbContextOptions =
                new DbContextOptionsBuilder<WeatheredContext>()
                .UseMongoDB(new MongoClient(AppSettings.WeatheredDbString), AppSettings.WeatheredDbName);
            using (var context = new WeatheredContext(dbContextOptions.Options))
            {
                // TODO: Maybe revisit this...
                //context.PastWeekStationData.Load();
                var dbStationLocs = context.PastWeekStationData;//.Local;
                if (dbStationLocs.Count() == 0)
                {
                    context.PastWeekStationData.AddRange(StationList.Stations);
                    context.SaveChanges();
                    Logging.Log("GetOutOfDateStations", "All Downloaded Station Data Inserted Into DB, was previously empty");
                    return;
                }
                if (dbStationLocs.Count() < StationList.Stations.Count)
                {
                    var newStations = StationList.Stations.Where(x => !dbStationLocs.Select(z => z.StationNumber).Contains(x.StationNumber)).ToList();
                    context.PastWeekStationData.AddRange(newStations);
                    Logging.Log("GetOutOfDateStations", $"Added {newStations.Count()} Previously Absent Stations And Data To DB");
                }
                var outOfDateStationData = dbStationLocs.Where(x => DateOnly.FromDateTime(x.LastUpdate) < DateOnly.FromDateTime(DateTime.Now));
                var updateCount = 0;
                foreach (PastWeekStationData station in outOfDateStationData)
                {
                    var stationListMatch = StationList.Stations.SingleOrDefault(x => x.StationNumber == station.StationNumber);
                    if (stationListMatch != null && stationListMatch.LastUpdate > station.LastUpdate)
                    {
                        station.PastWeekData = stationListMatch.PastWeekData;
                        updateCount++;
                    }
                }
                context.SaveChanges();
                Logging.Log("GetOutOfDateStations", $"Updated {updateCount} Stations And Data In DB");
                Logging.Log("GetOutOfDateStations", "Success");
            }
        }
        catch (Exception ex)
        {
            Logging.Log("GetOutOfDateStations", $"Failure - {ex.Message}");
            FailureShutdown();
        }
    }

    public static void FailureShutdown()
    {
        Logging.Log("GSOD Data Processor", "Failure, Shutting Down.", true);
        Environment.Exit(0);
    }
}
