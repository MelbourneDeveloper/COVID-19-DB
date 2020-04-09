using Covid19DB.Entities;
using System;

namespace Covid19DB.Repositories
{
    public interface IProvinceRepository : IRepository<Province>
    {
        Province GetOrInsert(string name, Guid regionId);
    }
}