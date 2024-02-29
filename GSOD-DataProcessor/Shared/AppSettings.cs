using Microsoft.Extensions.Configuration;

namespace GSOD_DataProcessor.Shared;

public static class AppSettings
{
    private static string? weatheredDbString = "";
    private static string? noaaGsodUri = "";
    public static string WeatheredDbString { get => weatheredDbString; }
    public static string NoaaGsodUri { get => noaaGsodUri; }
    public static void SetConfig(IConfiguration config)
    {
        weatheredDbString = config.GetConnectionString("weatheredDbString");
        noaaGsodUri = config.GetConnectionString("noaaGsodUri");
        MongoBase.SetupMongoBase();
    }
}
