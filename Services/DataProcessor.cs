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
                    int lastIndex = _bids.Count - 1;
                    var bestBidPrice = _bids.Keys[lastIndex];
                    var bestBidLevel = _bids.Values[lastIndex];

                    data[i].B0 = bestBidPrice;
                    data[i].BQ0 = bestBidLevel.QtySum;
                    data[i].BN0 = bestBidLevel.Count;
                }
                else
                {
                    data[i].B0 = null;
                    data[i].BQ0 = null;
                    data[i].BN0 = null;
                }

                if (_asks.Count > 0)
                {
                    var bestAskPrice = _asks.Keys[0];
                    var bestAskLevel = _asks.Values[0];

                    data[i].A0 = bestAskPrice;
                    data[i].AQ0 = bestAskLevel.QtySum;
                    data[i].AN0 = bestAskLevel.Count;
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
