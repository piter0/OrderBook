namespace SkyOrderBook.Interfaces
{
    public interface IFileReader<T>
    {
        IList<T> Read(string path);
    }
}