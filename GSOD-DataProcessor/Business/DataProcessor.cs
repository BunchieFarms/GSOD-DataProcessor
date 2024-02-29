using GSOD_DataProcessor.Models;
using GSOD_DataProcessor.Shared;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using MongoDB.Driver;
using System.Text;
using TinyCsvParser;

namespace GSOD_DataProcessor.Business;

public class DataProcessor
{
    public static async Task Start()
    {
        Logging.Log("GSOD Data Processor", "Start");
        bool updateAvailable = await NoaaSiteParsing.CheckNoaaSiteIfUpdateAvailable();
        if (!updateAvailable)
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
        var _stationCollection = MongoBase.WeatheredDB.GetCollection<PastWeekStationData>(Constants.PastWeekStationData);
        Logging.Log("GetOutOfDateStations", "Start");
        try
        {
            //var dbStationLocs = context.PastWeekStationData;
            long collectionCount = _stationCollection.CountDocuments(Builders<PastWeekStationData>.Filter.Empty);
            if (collectionCount == 0)
            {
                // TODO: This is naughty... Should probably break this up...
                _stationCollection.InsertManyAsync(StationList.Stations);
                Logging.Log("GetOutOfDateStations", "All Downloaded Station Data Inserted Into DB, was previously empty");
                return;
            }
            if (collectionCount < StationList.Stations.Count)
            {
                // TODO: Need to verify logic works the same...
                var newStations = StationList.Stations.Where(x => !_stationCollection.AsQueryable().Select(y => y.StationNumber).Contains(x.StationNumber)).ToList();
                _stationCollection.InsertManyAsync(newStations);
                //var newStations = StationList.Stations.Where(x => !dbStationLocs.Select(z => z.StationNumber).Contains(x.StationNumber)).ToList();
                //context.PastWeekStationData.AddRange(newStations);
                Logging.Log("GetOutOfDateStations", $"Added {newStations.Count()} Previously Absent Stations And Data To DB");
            }
            var outOfDateStationData = _stationCollection.AsQueryable().Where(x => DateOnly.FromDateTime(x.LastUpdate) < DateOnly.FromDateTime(DateTime.Now));
            var updateCount = 0;
            // TODO: Need to redo this logic to work with MongoDB, and probably break it up to save on memory
            //foreach (PastWeekStationData station in outOfDateStationData)
            //{
            //    var stationListMatch = StationList.Stations.SingleOrDefault(x => x.StationNumber == station.StationNumber);
            //    if (stationListMatch != null && stationListMatch.LastUpdate > station.LastUpdate)
            //    {
            //        station.PastWeekData = stationListMatch.PastWeekData;
            //        updateCount++;
            //    }
            //}
            //context.SaveChanges();
            Logging.Log("GetOutOfDateStations", $"Updated {updateCount} Stations And Data In DB");
            Logging.Log("GetOutOfDateStations", "Success");
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
