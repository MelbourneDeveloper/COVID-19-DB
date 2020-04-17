using Covid19DB.Db;
using Covid19DB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Covid19DB.Repositories
{
    public class LocationDayRepository : RepositoryBase<LocationDay>, ILocationDayRepository
    {

        public LocationDayRepository(Covid19DbContext covid19DbContext) : base(covid19DbContext)
        {
        }

        public LocationDay Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Insert(LocationDay item)
        {
            throw new NotImplementedException();
        }

        public LocationDay GetOrInsert(DateTimeOffset date, Location location, int? cases, int? deaths, int? recoveries)
        {
            if (location == null) throw new ArgumentNullException(nameof(location));

            var day = Get(date, location.Id);

            if (day != null) return day;

            day = new LocationDay
            {
                DateOfCount = date,
                NewCases = cases,
                Deaths = deaths,
                Location = location,
                Recoveries = recoveries
            };

            Covid19DbContext.LocationDays.Add(day);

            return day;
        }

        public LocationDay Get(DateTimeOffset date, Guid locationId)
        {
            return Covid19DbContext.LocationDays.FirstOrDefault(d =>
            d.DateOfCount == date &&
            d.Location.Id == locationId
            );
        }

#pragma warning disable CA1024 // Use properties where appropriate
        public IEnumerable<LocationDay> GetAll()
#pragma warning restore CA1024 // Use properties where appropriate
        {
            return Covid19DbContext.LocationDays;
        }
    }
}
