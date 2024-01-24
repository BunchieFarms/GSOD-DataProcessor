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
        public static void Start()
        {
            if (!NoaaSiteParsing.CheckNoaaSiteIfUpdateAvailable())
                return;

            GetNewestGSOD();
            DecompressGSOD();
            ParseGSODData();
            GetOutOfDateStations();

            File.Delete(NoaaArchive.NewestArchiveFileName);
            Directory.Delete(NoaaArchive.UncompressedFolderName, true);
        }

        public static void GetNewestGSOD()
        {
            using (var client = new HttpClient())
            {
                using (var s = client.GetStreamAsync(AppSettings.noaaGsodUri + NoaaArchive.NewestArchiveFileName))
                {
                    using (var fs = new FileStream(NoaaArchive.NewestArchiveFileName, FileMode.OpenOrCreate))
                    {
                        s.Result.CopyTo(fs);
                    }
                }
            }
        }

        public static void DecompressGSOD()
        {
            Stream inStream = File.OpenRead(NoaaArchive.NewestArchiveFileName);
            Stream gzipStream = new GZipInputStream(inStream);

            TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.ASCII);
            tarArchive.ExtractContents(NoaaArchive.UncompressedFolderName);
            tarArchive.Close();

            gzipStream.Close();
            inStream.Close();
        }

        public static void GetOutOfDateStations()
        {
            using (var context = new weatheredContext())
            {
                int stationLocsCount = context.StationLocs.Count();
                if (stationLocsCount == 0)
                {
                    //Just add everything to the DB and be done
                    context.StationLocs.AddRange(StationList.Stations);
                    context.SaveChanges();
                }
                else
                {
                    var outOfDateStations = context.PastWeekData.Include(x => x.StationLoc).Where(x => x.LastUpdated < DateOnly.FromDateTime(DateTime.Now));
                    foreach (PastWeekDatum station in outOfDateStations)
                    {
                        var stationListMatch = StationList.Stations.Single(x => x.StationNumber == station.StationLoc.StationNumber).PastWeekData;
                        if (stationListMatch.LastUpdated > station.LastUpdated)
                        {
                            station.PrecipData = stationListMatch.PrecipData;
                            station.TempData = stationListMatch.TempData;
                            station.LastUpdated = stationListMatch.LastUpdated;
                        }
                    }
                    context.SaveChanges();
                }
            }
        }

        public static void ParseGSODData()
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
        }
    }
}
