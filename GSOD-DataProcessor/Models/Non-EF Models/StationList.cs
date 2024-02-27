namespace GSOD_DataProcessor.Models;

public static class StationList
{
    private static readonly List<PastWeekStationData> _stationList = new();
    public static List<PastWeekStationData> Stations { get => _stationList; }
    public static void AddToStations(PastWeekStationData station)
    {
        _stationList.Add(station);
    }
}
