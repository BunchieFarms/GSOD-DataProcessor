﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable

using System.Text.Json;

namespace GSOD_DataProcessor.Models;

public partial class StationLoc
{
    public int StationLocId { get; set; }

    public string StationNumber { get; set; }

    public string Name { get; set; }

    public string Region { get; set; }

    public string Country { get; set; }

    public float Lat { get; set; }

    public float Lon { get; set; }

    public virtual PastWeekDatum PastWeekData { get; set; }

    public StationLoc() { }

    public StationLoc(List<StationGSOD> result)
    {
        StationNumber = result[0].Station;
        Lat = result[0].Latitude;
        Lon = result[0].Longitude;
        Name = result[0].Name;
        Region = "";
        Country = "";

        int commaIndex = result[0].Name.IndexOf(',');
        if (commaIndex > -1)
        {
            string regionCountry = result[0].Name.Substring(commaIndex + 1).Trim();
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

        List<DayValue> temps = new List<DayValue>();
        List<DayValue> prcps = new List<DayValue>();

        for (int i = 0; i < result.Count(); i++)
        {
            temps.Add(new DayValue(result[i].Date, ConvertMissingValue(result[i].Temp)));
            prcps.Add(new DayValue(result[i].Date, ConvertMissingValue(result[i].Prcp)));
        }
        PastWeekData = new PastWeekDatum
        {
            TempData = JsonSerializer.Serialize(temps),
            PrecipData = JsonSerializer.Serialize(prcps),
            LastUpdated = DateOnly.FromDateTime(result.Last().Date)
        };
    }

    private float ConvertMissingValue(float value)
    {
        float[] missingValues = { 9999.9f, 99.99f };
        return missingValues.Contains(value) ? 0 : value;
    }
}

public class DayValue
{
    public DateOnly Date { get; set; }
    public float Value { get; set; }

    public DayValue(DateTime date,  float value)
    {
        Date = DateOnly.FromDateTime(date);
        Value = value;
    }
}