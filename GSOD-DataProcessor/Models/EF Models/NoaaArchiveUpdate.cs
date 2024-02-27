using MongoDB.Bson;

namespace GSOD_DataProcessor.Models;

public partial class NoaaArchiveUpdate
{
    public ObjectId _id { get; set; } = ObjectId.GenerateNewId();
    public int NoaaArchiveUpdatesId { get; set; }
    public string Filename { get; set; }
    public DateTime Lastsiteupdate { get; set; }
}