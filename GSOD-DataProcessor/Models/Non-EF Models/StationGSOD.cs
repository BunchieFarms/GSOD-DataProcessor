namespace GSOD_DataProcessor.Models;

public class StationGSOD
{
    public string? StationNumber { get; set; }
    public DateTime Date { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public float Elevation { get; set; }
    public string? StationName { get; set; }
    public float Temp_Mean { get; set; }
    public string? Temp_Mean_Attribute { get; set; }
    public float Dewp_Mean { get; set; }
    public string? Dewp_Mean_Attribute { get; set; }
    public float StationPress_Mean { get; set; }
    public string? StationPress_Mean_Attribute { get; set; }
    public float SeaLevelPress_Mean { get; set; }
    public string? SeaLevelPress_Mean_Attribute { get; set; }
    public float Visi_Mean { get; set; }
    public string? Visi_Mean_Attribute { get; set; }
    public float WndSpd_Mean { get; set; }
    public string? WndSpd_Mean_Attribute { get; set; }
    public float WndSpd_Max { get; set; }
    public float WndSpd_Gust { get; set; }
    public float Temp_Max { get; set; }
    public string? Temp_Max_Attribute { get; set; }
    public float Temp_Min { get; set; }
    public string? Temp_Min_Attribute { get; set; }
    public float Precip_Total { get; set; }
    public string? Precip_Total_Attribute { get; set; }
    public float Snow_Depth { get; set; }
}
