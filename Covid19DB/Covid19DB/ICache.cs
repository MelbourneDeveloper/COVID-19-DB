namespace Covid19DB
{
    public interface ICache<T>
    {
        T Get(string key);
        void Add(string key, T value);
    }
}
