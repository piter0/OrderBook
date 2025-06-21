using Microsoft.Extensions.DependencyInjection;
using SkyOrderBook.Interfaces;
using SkyOrderBook.Models;
using System.Diagnostics;

namespace SkyOrderBook
{
    internal class Program
    {
        private static readonly string inputPath = "./ticks.csv";
        private static readonly string outputPath = "./ticks_results.csv";

        static void Main(string[] args)
        {
            var serviceProvider = Startup.ConfigureServices();

            var reader = serviceProvider.GetRequiredService<IFileReader<Record>>();
            var writer = serviceProvider.GetRequiredService<IFileWriter<Record>>();
            var processor = serviceProvider.GetRequiredService<IDataProcessor<Record>>();

            try
            {
                var records = reader.Read(inputPath);

                var sw = new Stopwatch();
                sw.Start();
                var results = processor.Process(records);
                sw.Stop();
                Console.WriteLine($"Total time [us]: {sw.ElapsedMilliseconds * 1000.0:F3}");
                Console.WriteLine($"Time per tick [us]: {sw.ElapsedMilliseconds * 1000.0 / records.Count:F3}");

                writer.Write(outputPath, records);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
            }
        }
    }
}
