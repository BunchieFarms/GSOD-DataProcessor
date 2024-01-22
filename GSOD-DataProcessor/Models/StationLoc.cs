﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using GSODDataProcessor;

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

    public virtual ICollection<PastWeekDatum> PastWeekData { get; set; } = new List<PastWeekDatum>();

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

        float[] temps = { 9999.9F, 9999.9F, 9999.9F, 9999.9F, 9999.9F, 9999.9F };
        float[] prcps = { 99.99F, 99.99F, 99.99F, 99.99F, 99.99F, 99.99F };

        for (int i = 0; i < result.Count(); i++)
        {
            temps[i] = result[i].Temp == 9999.9 ? 0 : result[i].Temp;
            prcps[i] = result[i].Prcp == 99.99 ? 0 : result[i].Prcp;
        }
        PastWeekData.Add(new PastWeekDatum
        {
            TempData = string.Join(",", temps),
            PrecipData = string.Join(",", prcps),
            LastUpdated = DateOnly.FromDateTime(result.Last().Date)
        });
    }
}