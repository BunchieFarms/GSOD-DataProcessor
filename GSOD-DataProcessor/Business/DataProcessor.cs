using GSOD_DataProcessor.Models;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.EntityFrameworkCore;
using System.Text;
using TinyCsvParser;

namespace GSOD_DataProcessor.Business
{
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
                DateTime startDate = DateTime.Now.AddDays(-7);

                var files = Directory.EnumerateFiles(NoaaArchive.UncompressedFolderName, "*.csv");

                foreach (var file in files)
                {
                    var result = csvParser
                        .ReadFromFile(file, Encoding.ASCII)
                        .Where(x => x.IsValid && x.Result.Date > startDate)
                        .Select(x => x.Result)
                        .ToList();

                    if (result.Count > 0)
                    {
                        StationList.AddToStations(new StationLoc(result));
                    }
                }
                Directory.Delete(NoaaArchive.UncompressedFolderName, true);
                Logging.Log("ParseGSODData", "Success");
            }
            catch (Exception ex)
            {
                Logging.Log("ParseGSODData", $"Failure - {ex.Message}");
            }
            
        }

        public static void GetOutOfDateStations()
        {
            Logging.Log("GetOutOfDateStations", "Start");
            try
            {
                using (var context = new weatheredContext())
                {
                    context.StationLocs.Include(x => x.PastWeekData).Load();
                    var dbStationLocs = context.StationLocs.Local;
                    if (dbStationLocs.Count() == 0)
                    {
                        context.StationLocs.AddRange(StationList.Stations);
                        context.SaveChanges();
                        Logging.Log("GetOutOfDateStations", "All Downloaded Station Data Inserted Into DB, was previously empty");
                        return;
                    }
                    if (dbStationLocs.Count() < StationList.Stations.Count)
                    {
                        var newStations = StationList.Stations.Where(x => !dbStationLocs.Select(z => z.StationNumber).Contains(x.StationNumber)).ToList();
                        var test = newStations.Count();
                        context.StationLocs.AddRange(newStations);
                        Logging.Log("GetOutOfDateStations", $"Added {newStations.Count()} Previously Absent Stations And Data To DB");
                    }
                    var outOfDateStations = dbStationLocs.Where(x => x.PastWeekData.LastUpdated < DateOnly.FromDateTime(DateTime.Now)).Select(x => x.PastWeekData);
                    var updateCount = 0;
                    foreach (PastWeekDatum station in outOfDateStations)
                    {
                        var stationListMatch = StationList.Stations.SingleOrDefault(x => x.StationNumber == station.StationLoc.StationNumber);
                        if (stationListMatch != null && stationListMatch.PastWeekData.LastUpdated > station.LastUpdated)
                        {
                            station.PrecipData = stationListMatch.PastWeekData.PrecipData;
                            station.TempData = stationListMatch.PastWeekData.TempData;
                            station.LastUpdated = stationListMatch.PastWeekData.LastUpdated;
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
            }
        }
    }
}
