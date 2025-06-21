namespace SkyOrderBook.Interfaces
{
    public interface IFileWriter<T>
    {
        void Write(string path, IList<T> data);
    }
}