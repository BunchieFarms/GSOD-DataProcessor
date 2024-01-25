namespace GSOD_DataProcessor.Models;

public partial class PastWeekDatum
{
    public int PastWeekDataId { get; set; }

    public int StationLocId { get; set; }

    public string TempData { get; set; }

    public string PrecipData { get; set; }

    public DateOnly? LastUpdated { get; set; }

    public virtual StationLoc StationLoc { get; set; }
}