using System;
using System.Collections.Generic;

namespace Covid19DB.Repositories
{
    public interface IRepository<T>
    {
        public T Get(Guid id);
        public void Insert(T item);
        IEnumerable<T> GetAll();
        void Update(T item);
    }
}
