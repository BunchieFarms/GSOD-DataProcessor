using GSOD_DataProcessor.Models;
using MongoDB.Driver;

namespace GSOD_DataProcessor.Shared;

internal static class MongoBase
{
    public static IMongoCollection<NoaaArchiveUpdate> NoaaArchUpdateColl;
    public static IMongoCollection<PastWeekStationData> PastStationDataColl;

    public static void SetupMongoBase()
    {
        IMongoDatabase WeatheredDB = new MongoClient(AppSettings.WeatheredDbString)
                        .GetDatabase(Constants.WeatheredDB);

        NoaaArchUpdateColl = WeatheredDB.GetCollection<NoaaArchiveUpdate>(Constants.NoaaArchiveUpdate);

        PastStationDataColl = WeatheredDB.GetCollection<PastWeekStationData>(Constants.PastWeekStationData);
    }
}
