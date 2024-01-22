using GSOD_DataProcessor.Models;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System.Text;
using TinyCsvParser;

namespace GSOD_DataProcessor
{
    public class DataProcessor
    {
        private readonly weatheredContext _weatheredContext;
        public DataProcessor(weatheredContext weatheredContext)
        {
            _weatheredContext = weatheredContext;
        }

        public void Start()
        {
            //Get newest gzip
            //Unzip gzip
            //Query Database for stations that have not updated yesterday
            //Only pick downloaded files that match above query
            //See if those downloaded files contain yesterday's data
            //update Database so yesterday is noted as updated

            GetNewestGSOD();
            DecompressGSOD();
            GetOutOfDateStations();

            File.Delete("2024.tar.gz");
            Directory.Delete("2024", true);
        }

        public void GetNewestGSOD()
        {
            using (var client = new HttpClient())
            {
                using (var s = client.GetStreamAsync("noaaDir"))
                {
                    using (var fs = new FileStream("2024.tar.gz", FileMode.OpenOrCreate))
                    {
                        s.Result.CopyTo(fs);
                    }
                }
            }
        }

        public void DecompressGSOD()
        {
            Stream inStream = File.OpenRead("2024.tar.gz");
            Stream gzipStream = new GZipInputStream(inStream);

            TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.ASCII);
            tarArchive.ExtractContents("2024");
            tarArchive.Close();

            gzipStream.Close();
            inStream.Close();
        }

        public void GetOutOfDateStations()
        {
            int stationLocsCount = _weatheredContext.StationLocs.Count();
            if (stationLocsCount == 0)
            {
                InitializeStationsAndPastWeatherTable();
            }
            else
            {
                var outOfDateStations = _weatheredContext.PastWeekData.Where(x => x.LastUpdated < DateOnly.FromDateTime(DateTime.Now.AddDays(-1)));
            }

        }

        public void InitializeStationsAndPastWeatherTable()
        {
            CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
            CsvStationGSODMapping csvMapper = new CsvStationGSODMapping();
            CsvParser<StationGSOD> csvParser = new CsvParser<StationGSOD>(csvParserOptions, csvMapper);
            DateTime startDate = DateTime.Now.AddDays(-7);

            var files = Directory.EnumerateFiles("2024", "*.csv");
            List<StationLoc> stationList = new List<StationLoc>();

            foreach (var file in files)
            {
                var result = csvParser
                    .ReadFromFile(file, Encoding.ASCII)
                    .Where(x => x.IsValid && x.Result.Date > startDate)
                    .Select(x => x.Result)
                    .ToList();

                if (result.Count > 0)
                {
                    stationList.Add(new StationLoc(result));
                }
            }

            _weatheredContext.StationLocs.AddRange(stationList);
            _weatheredContext.SaveChanges();
        }
    }
}
