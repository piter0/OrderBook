using CsvHelper.Configuration.Attributes;

namespace SkyOrderBook.Models
{
    internal sealed class Record
    {
        [Index(0)]
        public long SourceTime { get; set; }
        [Index(1)]
        public int? Side { get; set; }
        [Index(2)]
        public char Action { get; set; }
        [Index(3)]
        public long OrderId { get; set; }
        [Index(4)]
        public decimal Price { get; set; }
        [Index(5)]
        public int Qty { get; set; }
        [Optional]
        public decimal? B0 { get; set; }
        [Optional]
        public int? BQ0 { get; set; }
        [Optional]
        public int? BN0 { get; set; }
        [Optional]
        public decimal? A0 { get; set; }
        [Optional]
        public int? AQ0 { get; set; }
        [Optional]
        public int? AN0 { get; set; }
    }
}
