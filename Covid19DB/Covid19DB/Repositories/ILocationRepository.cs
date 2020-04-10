using Covid19DB.Entities;
using System;

namespace Covid19DB.Repositories
{
    public interface ILocationRepository
    {
        Location Get(Guid Id);
        Location GetOrInsert(string locationName, Province province, decimal? latitude, decimal? longitude);
        void Insert(Location item);
    }
}