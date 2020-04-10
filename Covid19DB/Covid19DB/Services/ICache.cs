namespace Covid19DB.Services
{
    public interface ICache<T>
    {
        T Get(string key);
        void Add(string key, T value);
    }
}
