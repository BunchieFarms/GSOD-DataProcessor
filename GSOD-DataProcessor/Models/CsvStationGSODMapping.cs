using TinyCsvParser.Mapping;

namespace GSOD_DataProcessor.Models;

public class CsvStationGSODMapping : CsvMapping<StationGSOD>
{
    public CsvStationGSODMapping()
        : base()
    {
        MapProperty(0, x => x.StationNumber);
        MapProperty(1, x => x.Date);
        MapProperty(2, x => x.Latitude);
        MapProperty(3, x => x.Longitude);
        MapProperty(4, x => x.Elevation);
        MapProperty(5, x => x.StationName);
        MapProperty(6, x => x.Temp_Mean);
        MapProperty(7, x => x.Temp_Mean_Attribute);
        MapProperty(8, x => x.Dewp_Mean);
        MapProperty(9, x => x.Dewp_Mean_Attribute);
        MapProperty(10, x => x.SeaLevelPress_Mean);
        MapProperty(11, x => x.SeaLevelPress_Mean_Attribute);
        MapProperty(12, x => x.StationPress_Mean);
        MapProperty(13, x => x.StationPress_Mean_Attribute);
        MapProperty(14, x => x.Visi_Mean);
        MapProperty(15, x => x.Visi_Mean_Attribute);
        MapProperty(16, x => x.WndSpd_Mean);
        MapProperty(17, x => x.WndSpd_Mean_Attribute);
        MapProperty(18, x => x.WndSpd_Max);
        MapProperty(19, x => x.WndSpd_Gust);
        MapProperty(20, x => x.Temp_Max);
        MapProperty(21, x => x.Temp_Max_Attribute);
        MapProperty(22, x => x.Temp_Min);
        MapProperty(23, x => x.Temp_Min_Attribute);
        MapProperty(24, x => x.Precip_Total);
        MapProperty(25, x => x.Precip_Total_Attribute);
        MapProperty(26, x => x.Snow_Depth);
    }
}
