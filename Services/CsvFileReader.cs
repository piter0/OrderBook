using CsvHelper;
using CsvHelper.Configuration;
using SkyOrderBook.Interfaces;
using SkyOrderBook.Models;
using System.Globalization;

namespace SkyOrderBook.Services
{
    internal sealed class CsvFileReader : IFileReader<Record>
    {
        public IList<Record> Read(string path)
        {
            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                MissingFieldFound = null
            });

            return [.. csv.GetRecords<Record>()];
        }
    }
}
