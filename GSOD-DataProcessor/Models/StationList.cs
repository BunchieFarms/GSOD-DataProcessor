namespace GSOD_DataProcessor.Models;

public static class StationList
{
    private static List<PastWeekStationData> _stationList = new();
    public static List<PastWeekStationData> Stations { get => _stationList; }
    public static void AddToStations(PastWeekStationData station)
    {
        _stationList.Add(station);
    }
    public static IEnumerable<PastWeekStationData> RemoveNewInsertsFromList(List<PastWeekStationData> newStations)
    {
        _stationList = _stationList.Where(x => !newStations.Select(x => x.StationNumber).Contains(x.StationNumber)).ToList();
        return _stationList;
    }
    public static void PurgeStationList()
    {
        _stationList.Clear();
    }
}
