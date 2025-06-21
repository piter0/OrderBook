namespace SkyOrderBook.Interfaces
{
    public interface IDataProcessor<T>
    {
        IList<T> Process(IList<T> data);
    }
}