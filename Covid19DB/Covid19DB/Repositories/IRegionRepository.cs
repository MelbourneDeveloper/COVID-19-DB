using Covid19DB.Entities;
using System;

namespace Covid19DB.Repositories
{
    public interface IRegionRepository
    {
        Region Get(Guid Id);
        Region Get(string name);
        Region GetOrInsert(string regionName);
        void Insert(Region item);
    }
}