﻿using MongoDB.Bson;

namespace GSOD_DataProcessor.Models;

public class PastWeekStationData
{
    public ObjectId _id { get; set; } = ObjectId.GenerateNewId();
    public string? StationNumber { get; set; }
    public string? StationName { get; set; }
    public string Region { get; set; } = "";
    public string Country { get; set; } = "";
    // TODO: Convert all floats to lower precision
    public float Latitude { get; set; }
    // TODO: Convert all floats to lower precision
    public float Longitude { get; set; }
    // TODO: Convert all floats to lower precision
    public float Elevation { get; set; }
    // TODO: LastUpdate is going into the db as the Minimum Date
    public DateTime LastUpdate { get; set; }
    public List<PastWeekData> PastWeekData { get; set; } = new();

    public PastWeekStationData() { }

    public PastWeekStationData(List<StationGSOD> station)
    {
        StationNumber = station[0].StationNumber;
        StationName = station[0].StationName;
        Latitude = station[0].Latitude;
        Longitude = station[0].Longitude;
        Elevation = station[0].Elevation;

        int commaIndex = station[0].StationName.IndexOf(',');
        if (commaIndex > -1)
        {
            string regionCountry = station[0].StationName.Substring(commaIndex + 1).Trim();
            string[] splitRegCo = regionCountry.Split(' ');
            if (splitRegCo.Length > 1)
            {
                Region = splitRegCo[0];
                Country = splitRegCo[1];
            }
            else
            {
                Country = splitRegCo[0];
            }
        }

        foreach (StationGSOD day in station)
        {
            PastWeekData.Add(new PastWeekData(day));
        }
    }
}

public class PastWeekData
{
    public DateTime Date { get; set; }
    public DataItems? Temp_Mean { get; set; }
    public DataItems? Temp_Max { get; set; }
    public DataItems? Temp_Min { get; set; }
    public DataItems? SeaLevelPress_Mean { get; set; }
    public DataItems? StationPress_Mean { get; set; }
    public DataItems? Visi_Mean { get; set; }
    public DataItems? Dewp_Mean { get; set; }
    public DataItems? WndSpd_Mean { get; set; }
    public DataItems? WndSpd_Max { get; set; }
    public DataItems? WndSpd_Gust { get; set; }
    public DataItems? Precip_Total { get; set; }
    public DataItems? Snow_Depth { get; set; }

    public PastWeekData() { }

    public PastWeekData(StationGSOD day)
    {
        Date = day.Date;
        Temp_Mean = new DataItems(day.Temp_Mean, day.Temp_Mean_Attribute);
        Temp_Max = new DataItems(day.Temp_Max, day.Temp_Max_Attribute);
        Temp_Min = new DataItems(day.Temp_Min, day.Temp_Min_Attribute);
        SeaLevelPress_Mean = new DataItems(day.SeaLevelPress_Mean, day.SeaLevelPress_Mean_Attribute);
        StationPress_Mean = new DataItems(day.StationPress_Mean, day.StationPress_Mean_Attribute);
        Visi_Mean = new DataItems(day.Visi_Mean, day.Visi_Mean_Attribute);
        Dewp_Mean = new DataItems(day.Dewp_Mean, day.Dewp_Mean_Attribute);
        WndSpd_Mean = new DataItems(day.WndSpd_Mean, day.WndSpd_Mean_Attribute);
        WndSpd_Max = new DataItems(day.WndSpd_Max);
        WndSpd_Gust = new DataItems(day.WndSpd_Gust);
        Precip_Total = new DataItems(day.Precip_Total, day.Precip_Total_Attribute);
        Snow_Depth = new DataItems(day.Snow_Depth);
    }
}

public class DataItems
{
    // TODO: Convert all floats to lower precision
    public float Value { get; set; }
    public string? Attribute { get; set; }
    public DataItems() { }
    public DataItems(float value, string attribute = "")
    {
        Value = value;
        Attribute = attribute;
    }
    // TODO: Convert Missing values (99.99 and similar) to 0
}