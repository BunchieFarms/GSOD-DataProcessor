using GSOD_DataProcessor.Business;
using GSOD_DataProcessor.Shared;
using Microsoft.Extensions.Configuration;

namespace GSODDataProcessor;

public class Program
{
    public static async Task Main()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.json", false);
        var config = configuration.Build();

        AppSettings.SetConfig(config);

        await DataProcessor.Start();
    }
}