using SkyOrderBook.Interfaces;
using SkyOrderBook.Models;

namespace SkyOrderBook.Services
{
    internal sealed class DataProcessor : IDataProcessor<Record>
    {
        private readonly Dictionary<decimal, Dictionary<long, int>> _bids = [];
        private readonly Dictionary<decimal, Dictionary<long, int>> _asks = [];

        public IList<Record> Process(IList<Record> data)
        {
            var output = new List<Record>();
            var dataLength = data.Count;

            for (var i = 0; i < dataLength; i++)
            {
                if (data[i].Action == 'Y' || data[i].Action == 'F')
                {
                    _bids.Clear();
                    _asks.Clear();
                    continue;
                }
                else if (data[i].Action == 'A' || data[i].Action == 'M')
                {
                    var book = GetBook(data[i].Side);

                    foreach (var priceLevel in book.Values)
                    {
                        if (priceLevel.ContainsKey(data[i].OrderId))
                        {
                            priceLevel.Remove(data[i].OrderId);
                            break;
                        }
                    }

                    if (!book.TryGetValue(data[i].Price, out Dictionary<long, int>? value))
                    {
                        value = [];
                        book[data[i].Price] = value;
                    }

                    if (!value.ContainsKey(data[i].OrderId))
                    {
                        value.Add(data[i].OrderId, data[i].Qty);
                    }
                }
                else
                {
                    var book = GetBook(data[i].Side);

                    if (book.TryGetValue(data[i].Price, out Dictionary<long, int>? value)
                        && value.ContainsKey(data[i].OrderId))
                    {
                        value.Remove(data[i].OrderId);
                        if (book[data[i].Price].Count == 0)
                        {
                            book.Remove(data[i].Price);
                        }
                    }
                }

                data[i].B0 = GetBestPrice(_bids);
                data[i].BQ0 = GetOrdersQtySum(_bids, data[i].B0);
                data[i].BN0 = GetOrdersCount(_bids, data[i].B0);

                data[i].A0 = GetBestPrice(_asks, false);
                data[i].AQ0 = GetOrdersQtySum(_asks, data[i].A0);
                data[i].AN0 = GetOrdersCount(_asks, data[i].A0);

                output.Add(data[i]);
            }

            return output;
        }

        private Dictionary<decimal, Dictionary<long, int>> GetBook(int? side) => side == 1 ? _bids : _asks;

        private static decimal? GetBestPrice(Dictionary<decimal, Dictionary<long, int>> book, bool isBid = true)
        {
            if (IsEmpty(book))
            {
                return null;
            }

            return isBid ? book.Keys.Max() : book.Keys.Min();
        }

        private static int? GetOrdersQtySum(Dictionary<decimal, Dictionary<long, int>> book, decimal? bestPrice)
        {
            if (IsEmpty(book))
            {
                return null;
            }

            var sum = 0;

            if (bestPrice is not null)
            {
                var bestPriceOrders = book[(decimal)bestPrice];

                foreach (var order in bestPriceOrders)
                {
                    sum += order.Value;
                }
            }

            return sum;
        }

        private static int? GetOrdersCount(Dictionary<decimal, Dictionary<long, int>> book, decimal? bestPrice)
        {
            if (IsEmpty(book))
            {
                return null;
            }

            var sum = 0;

            if (bestPrice is not null)
            {
                var bestPriceOrders = book[(decimal)bestPrice];

                foreach (var order in bestPriceOrders)
                {
                    sum++;
                }
            }

            return sum;
        }

        private static bool IsEmpty(Dictionary<decimal, Dictionary<long, int>> book) => book.Count == 0;
    }
}
