using Covid19DB.Entities;
using System;

namespace Covid19DB.Repositories
{
    public interface ILocationDayRepository : IRepository<LocationDay>
    {
        LocationDay Get(DateTimeOffset date, Guid locationId);
        LocationDay GetOrInsertLocationDay(DateTimeOffset date, Guid locationId, int? cases, int? deaths, int? recoveries);
    }
}