using GSOD_DataProcessor.Models;
using MongoDB.Driver;

namespace GSOD_DataProcessor.Shared;

internal static class MongoBase
{
    public static IMongoDatabase WeatheredDB;

    public static void SetupMongoBase()
    {
        WeatheredDB = new MongoClient(AppSettings.WeatheredDbString)
                        .GetDatabase(Constants.WeatheredDB);
    }
}
