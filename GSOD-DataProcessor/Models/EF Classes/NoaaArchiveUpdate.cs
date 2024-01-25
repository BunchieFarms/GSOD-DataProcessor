namespace GSOD_DataProcessor.Models;

public partial class NoaaArchiveUpdate
{
    public int NoaaArchiveUpdatesId { get; set; }

    public string Filename { get; set; }

    public DateTime Lastsiteupdate { get; set; }
}