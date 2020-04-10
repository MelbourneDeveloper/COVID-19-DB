using Covid19DB.Db;
using Covid19DB.Entities;
using System;
using System.Linq;

namespace Covid19DB.Repositories
{
    public class LocationDayRepository : ILocationDayRepository
    {
        private readonly Covid19DbContext _covid19DbContext;

        public LocationDayRepository(Covid19DbContext covid19DbContext)
        {
            _covid19DbContext = covid19DbContext;
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
                Date = date,
                NewCases = cases,
                Deaths = deaths,
                Location = location,
                Recoveries = recoveries
            };

            _covid19DbContext.LocationDays.Add(day);

            return day;
        }

        public LocationDay Get(DateTimeOffset date, Guid locationId)
        {
            return _covid19DbContext.LocationDays.FirstOrDefault(d =>
            d.Date == date &&
            d.Location.Id == locationId
            );
        }
    }
}
