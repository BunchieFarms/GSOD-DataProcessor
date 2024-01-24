namespace GSOD_DataProcessor.Models
{
    public static class StationList
    {
        private static readonly List<StationLoc> _stationList = new();
        public static List<StationLoc> Stations { get => _stationList; }
        public static void AddToStations(StationLoc station)
        {
            _stationList.Add(station);
        }
        public static void AddToStations(List<StationLoc> stations)
        {
            _stationList.AddRange(stations);
        }
    }
}
