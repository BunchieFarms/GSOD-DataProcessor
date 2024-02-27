using Microsoft.Extensions.Configuration;

namespace GSOD_DataProcessor.Models;

public static class AppSettings
{
    private static string? weatheredDbString = "";
    private static string? weatheredDbName = "";
    private static string? noaaGsodUri = "";
    public static string WeatheredDbString { get => weatheredDbString; }
    public static string WeatheredDbName { get => weatheredDbName; }
    public static string NoaaGsodUri { get => noaaGsodUri; }
    public static void SetConfig(IConfiguration config)
    {
        weatheredDbString = config.GetConnectionString("weatheredDbString");
        weatheredDbName = config.GetConnectionString("weatheredDbName");
        noaaGsodUri = config.GetConnectionString("noaaGsodUri");
    }
}
