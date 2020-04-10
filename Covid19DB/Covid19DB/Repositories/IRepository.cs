using System;

namespace Covid19DB.Repositories
{
    public interface IRepository<T>
    {
        public T Get(Guid id);
        public void Insert(T item);
    }
}
