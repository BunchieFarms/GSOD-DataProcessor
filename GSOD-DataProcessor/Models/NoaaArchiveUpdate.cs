using MongoDB.Bson;

namespace GSOD_DataProcessor.Models;

public partial class NoaaArchiveUpdate
{
    public ObjectId _id { get; set; } = ObjectId.GenerateNewId();
    public string? FileName { get; set; }
    public DateTime LastSiteUpdate { get; set; }
}