using Covid19DB.Entities;
using System;

namespace Covid19DB.Repositories
{
    public interface ILocationDayRepository : IRepository<LocationDay>
    {
        LocationDay Get(DateTimeOffset date, Guid locationId);
        LocationDay GetOrInsert(DateTimeOffset date, Location location, int? cases, int? deaths, int? recoveries);
    }
}