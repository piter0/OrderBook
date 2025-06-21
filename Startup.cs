using Microsoft.Extensions.DependencyInjection;
using SkyOrderBook.Interfaces;
using SkyOrderBook.Models;
using SkyOrderBook.Services;

namespace SkyOrderBook
{
    public static class Startup
    {
        public static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IFileReader<Record>, CsvFileReader>();
            services.AddSingleton<IFileWriter<Record>, CsvFileWriter>();
            services.AddSingleton<IDataProcessor<Record>, DataProcessor>();

            return services.BuildServiceProvider();
        }
    }
}
