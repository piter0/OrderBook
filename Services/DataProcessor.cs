using SkyOrderBook.Interfaces;
using SkyOrderBook.Models;

namespace SkyOrderBook.Services
{
    internal sealed class DataProcessor : IDataProcessor<Record>
    {
        private readonly Dictionary<long, OrderDetails> _orders = [];
        private readonly Dictionary<decimal, PriceLevel> _bids = [];
        private readonly SortedSet<decimal> _bidPrices = new(Comparer<decimal>.Create((a, b) => a.CompareTo(b)));

        private readonly Dictionary<decimal, PriceLevel> _asks = [];
        private readonly SortedSet<decimal> _askPrices = new(Comparer<decimal>.Create((a, b) => a.CompareTo(b)));

        public IList<Record> Process(IList<Record> data)
        {
            var dataLength = data.Count;

            for (var i = 0; i < dataLength; i++)
            {

                switch (data[i].Action)
                {
                    case 'Y':
                    case 'F':
                        _orders.Clear();
                        _bids.Clear();
                        _asks.Clear();
                        _bidPrices.Clear();
                        _askPrices.Clear();
                        break;

                    case 'A':
                    case 'M':
                        var (book, prices) = GetBook(data[i].Side);
                        RemoveOrder(data[i].OrderId, book, prices);
                        AddOrUpdateOrder(data[i], book, prices);
                        break;

                    default:
                        (book, prices) = GetBook(data[i].Side);
                        RemoveOrder(data[i].OrderId, book, prices);
                        break;
                }

                if (_bidPrices.Count > 0)
                {
                    var bestBid = _bidPrices.Max;
                    var level = _bids[bestBid];
                    data[i].B0 = bestBid;
                    data[i].BQ0 = level.QtySum;
                    data[i].BN0 = level.Count;
                }
                else
                {
                    data[i].B0 = null;
                    data[i].BQ0 = null;
                    data[i].BN0 = null;
                }

                if (_askPrices.Count > 0)
                {
                    var bestAsk = _askPrices.Min;
                    var level = _asks[bestAsk];
                    data[i].A0 = bestAsk;
                    data[i].AQ0 = level.QtySum;
                    data[i].AN0 = level.Count;
                }
                else
                {
                    data[i].A0 = null;
                    data[i].AQ0 = null;
                    data[i].AN0 = null;
                }
            }

            return data;
        }

        private (Dictionary<decimal, PriceLevel> book, SortedSet<decimal> prices) GetBook(int? side)
                => side == 1 ? (_bids, _bidPrices) : (_asks, _askPrices);

        private void RemoveOrder(long orderId, Dictionary<decimal, PriceLevel> book, SortedSet<decimal> prices)
        {
            if (_orders.TryGetValue(orderId, out var existing))
            {
                if (book.TryGetValue(existing.Price, out var level))
                {
                    level.QtySum -= existing.Qty;
                    level.Count--;
                    if (level.Count == 0)
                    {
                        book.Remove(existing.Price);
                        prices.Remove(existing.Price);
                    }
                }
                _orders.Remove(orderId);
            }
        }

        private void AddOrUpdateOrder(Record record, Dictionary<decimal, PriceLevel> book, SortedSet<decimal> prices)
        {
            if (!book.TryGetValue(record.Price, out var level))
            {
                level = new PriceLevel();
                book[record.Price] = level;
                prices.Add(record.Price);
            }

            level.QtySum += record.Qty;
            level.Count++;

            _orders[record.OrderId] = new OrderDetails
            {
                Side = record.Side,
                Price = record.Price,
                Qty = record.Qty
            };
        }
    }
}
