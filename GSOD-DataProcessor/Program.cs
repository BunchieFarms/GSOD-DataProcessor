using GSOD_DataProcessor;
using GSOD_DataProcessor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GSODDataProcessor
{
    public class Program
    {
        private static weatheredContext _weatheredContext;
        public static void Main()
        {
            var services = new ServiceCollection();
            services.AddDbContext<weatheredContext>(options => options.UseNpgsql("DBConn"));
            var serviceProvider = services.BuildServiceProvider();
            _weatheredContext = serviceProvider.GetService<weatheredContext>();
            DataProcessor dataProcessor = new DataProcessor(_weatheredContext);
            dataProcessor.Start();
        }
    }
}