using GSODDataProcessor;
using TinyCsvParser.Mapping;

namespace GSOD_DataProcessor.Models
{
    public class CsvStationGSODMapping : CsvMapping<StationGSOD>
    {
        public CsvStationGSODMapping()
            : base()
        {
            MapProperty(0, x => x.Station);
            MapProperty(1, x => x.Date);
            MapProperty(2, x => x.Latitude);
            MapProperty(3, x => x.Longitude);
            MapProperty(5, x => x.Name);
            MapProperty(6, x => x.Temp);
            MapProperty(24, x => x.Prcp);
        }
    }
}
