using Microsoft.Extensions.Configuration;

namespace GSOD_DataProcessor.Models
{
    public static class AppSettings
    {
        public static string? weatheredDbString;
        public static string? noaaGsodUri;
        public static void SetConfig(IConfiguration config)
        {
            weatheredDbString = config.GetConnectionString("weatheredDbString");
            noaaGsodUri = config.GetConnectionString("noaaGsodUri");
        }
    }
}
