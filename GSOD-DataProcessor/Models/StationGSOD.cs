namespace GSOD_DataProcessor.Models;

public class StationGSOD
{
    public string? StationNumber { get; set; }
    public DateTime Date { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Elevation { get; set; }
    public string? StationName { get; set; }
    public double Temp_Mean { get; set; }
    public string? Temp_Mean_Attribute { get; set; }
    public double Dewp_Mean { get; set; }
    public string? Dewp_Mean_Attribute { get; set; }
    public double StationPress_Mean { get; set; }
    public string? StationPress_Mean_Attribute { get; set; }
    public double SeaLevelPress_Mean { get; set; }
    public string? SeaLevelPress_Mean_Attribute { get; set; }
    public double Visi_Mean { get; set; }
    public string? Visi_Mean_Attribute { get; set; }
    public double WndSpd_Mean { get; set; }
    public string? WndSpd_Mean_Attribute { get; set; }
    public double WndSpd_Max { get; set; }
    public double WndSpd_Gust { get; set; }
    public double Temp_Max { get; set; }
    public string? Temp_Max_Attribute { get; set; }
    public double Temp_Min { get; set; }
    public string? Temp_Min_Attribute { get; set; }
    public double Precip_Total { get; set; }
    public string? Precip_Total_Attribute { get; set; }
    public double Snow_Depth { get; set; }
}
