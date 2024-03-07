namespace GSOD_DataProcessor.Models;

public static class StationList
{
    private static List<PastWeekStationData> _stationList = new();
    private static Dictionary<string, PastWeekStationData> _newStationDict = new();
    public static List<PastWeekStationData> Stations { get => _stationList; }
    public static void AddToStations(PastWeekStationData station)
    {
        _stationList.Add(station);
    }
    public static Dictionary<string, PastWeekStationData> RemoveNewInsertsFromList(List<PastWeekStationData> newStations)
    {
        _stationList = _stationList.Where(x => !newStations.Select(x => x.StationNumber).Contains(x.StationNumber)).ToList();
        _newStationDict.Clear();
        foreach (PastWeekStationData pastWeekStationData in _stationList)
        {
            _newStationDict.Add(pastWeekStationData.StationNumber, pastWeekStationData);
        }
        return _newStationDict;
    }
    public static void PurgeStationList()
    {
        _stationList.Clear();
    }
}
