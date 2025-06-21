using CsvHelper;
using CsvHelper.Configuration;
using SkyOrderBook.Interfaces;
using SkyOrderBook.Models;
using System.Globalization;

namespace SkyOrderBook.Services
{
    internal sealed class CsvFileWriter : IFileWriter<Record>
    {
        public void Write(string path, IList<Record> data)
        {
            using var writer = new StreamWriter(path);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                MissingFieldFound = null
            });

            csv.WriteRecords(data);
        }
    }
}