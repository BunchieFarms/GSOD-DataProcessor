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
        await ParseAndProcessGSODData();
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

    // TODO: Improve logging
    public static async Task ParseAndProcessGSODData()
    {
        Logging.Log("ParseAndProcessGSODData", "Start");
        try
        {
            CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
            CsvStationGSODMapping csvMapper = new CsvStationGSODMapping();
            CsvParser<StationGSOD> csvParser = new CsvParser<StationGSOD>(csvParserOptions, csvMapper);
            DateOnly startDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-8));

            var files = Directory.EnumerateFiles(NoaaArchive.UncompressedFolderName, "*.csv");

            bool isInitialSetup = MongoBase.PastStationDataColl.CountDocuments(Builders<PastWeekStationData>.Filter.Empty) == 0;

            for (var i = 0; i < files.Count(); i++)
            {
                var result = csvParser
                    .ReadFromFile(files.ElementAt(i), Encoding.ASCII)
                    .Where(x => x.IsValid && DateOnly.FromDateTime(x.Result.Date) > startDate)
                    .Select(x => x.Result)
                    .ToList();

                if (result.Count > 0)
                    StationList.AddToStations(new PastWeekStationData(result));

                // We will just keep 1000 in memory at a time, and insert/update max 1000 at a time.
                if (StationList.Stations.Count == 1000 || i == files.Count() - 1)
                {
                    if (isInitialSetup)
                        await AddToEmptyCollection();
                    else
                        await UpdateCollection();

                    StationList.PurgeStationList();
                }
            }
            Directory.Delete(NoaaArchive.UncompressedFolderName, true);
            Logging.Log("ParseAndProcessGSODData", "Success");
        }
        catch (Exception ex)
        {
            Logging.Log("ParseAndProcessGSODData", $"Failure - {ex.Message}");
            FailureShutdown();
        }
    }

    // TODO: Improve logging
    public static async Task AddToEmptyCollection()
    {
        Logging.Log("AddToEmptyCollection", "Start");
        try
        {
            await MongoBase.PastStationDataColl.InsertManyAsync(StationList.Stations);
            Logging.Log("AddToEmptyCollection", $"Successfully added {StationList.Stations.Count} rows to collection.");
            Logging.Log("AddToEmptyCollection", "Success");
        }
        catch (Exception ex)
        {
            Logging.Log("AddToEmptyCollection", $"Failure - {ex.Message}");
            FailureShutdown();
        }
    }

    // TODO: Improve logging
    public static async Task UpdateCollection()
    {
        Logging.Log("UpdateCollection", "Start");
        try
        {
            var existingStationNumbers = MongoBase.PastStationDataColl.AsQueryable().Select(y => y.StationNumber).ToList();
            var newStations = StationList.Stations.Where(x => !existingStationNumbers.Contains(x.StationNumber)).ToList();
            if (newStations.Count > 0)
            {
                await MongoBase.PastStationDataColl.InsertManyAsync(newStations);
                Logging.Log("UpdateCollection", $"Added {newStations.Count} Previously Absent Stations And Data To Collection.");
            }

            var updateCount = 0;
            Dictionary<string, PastWeekStationData> newStationDict = StationList.RemoveNewInsertsFromList(newStations);
            var bulkOps = new List<WriteModel<PastWeekStationData>>();
            foreach (var station in StationList.Stations)
            {
                var stationFilter = Builders<PastWeekStationData>.Filter.Eq(x => x.StationNumber, station.StationNumber);
                var stationUpdate = Builders<PastWeekStationData>.Update
                        .Set(x => x.PastWeekData, newStationDict[station.StationNumber].PastWeekData)
                        .Set(x => x.LastUpdate, newStationDict[station.StationNumber].LastUpdate);
                var updateOne = new UpdateOneModel<PastWeekStationData>(stationFilter, stationUpdate) { IsUpsert = true };

                bulkOps.Add(updateOne);
                updateCount++;
            }
            await MongoBase.PastStationDataColl.BulkWriteAsync(bulkOps);
            Logging.Log("UpdateCollection", $"Updated {updateCount} Stations And Data In DB");
            Logging.Log("UpdateCollection", "Success");
        }
        catch (Exception ex)
        {
            Logging.Log("UpdateCollection", $"Failure - {ex.Message}");
            FailureShutdown();
        }
    }

    public static void FailureShutdown()
    {
        Logging.Log("GSOD Data Processor", "Failure, Shutting Down.", true);
        Environment.Exit(0);
    }
}
