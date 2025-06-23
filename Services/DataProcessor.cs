using SkyOrderBook.Interfaces;
using SkyOrderBook.Models;

namespace SkyOrderBook.Services
{
    internal sealed class DataProcessor : IDataProcessor<Record>
    {
        private readonly Dictionary<long, OrderDetails> _orders = [];
        private readonly SortedList<decimal, PriceLevel> _bids = [];
        private readonly SortedList<decimal, PriceLevel> _asks = [];

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
                        break;

                    case 'A':
                    case 'M':
                        var book = GetBook(data[i].Side);
                        RemoveOrder(data[i].OrderId, book);
                        AddOrUpdateOrder(data[i], book);
                        break;

                    default:
                        book = GetBook(data[i].Side);
                        RemoveOrder(data[i].OrderId, book);
                        break;
                }

                if (_bids.Count > 0)
                {
                    var bestBid = _bids.Last();
                    data[i].B0 = bestBid.Key;
                    data[i].BQ0 = bestBid.Value.QtySum;
                    data[i].BN0 = bestBid.Value.Count;
                }
                else
                {
                    data[i].B0 = null;
                    data[i].BQ0 = null;
                    data[i].BN0 = null;
                }

                if (_asks.Count > 0)
                {
                    var bestAsk = _asks.First();
                    data[i].A0 = bestAsk.Key;
                    data[i].AQ0 = bestAsk.Value.QtySum;
                    data[i].AN0 = bestAsk.Value.Count;
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

        private SortedList<decimal, PriceLevel> GetBook(int? side) => side == 1 ? _bids : _asks;

        private void RemoveOrder(long orderId, SortedList<decimal, PriceLevel> book)
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
                    }
                }
                _orders.Remove(orderId);
            }
        }

        private void AddOrUpdateOrder(Record record, SortedList<decimal, PriceLevel> book)
        {
            if (!book.TryGetValue(record.Price, out var level))
            {
                level = new PriceLevel();
                book[record.Price] = level;
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
