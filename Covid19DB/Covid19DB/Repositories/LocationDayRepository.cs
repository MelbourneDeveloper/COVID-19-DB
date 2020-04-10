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

        public LocationDay Get(Guid Id)
        {
            throw new NotImplementedException();
        }

        public void Insert(LocationDay item)
        {
            throw new NotImplementedException();
        }

        public LocationDay GetOrInsertLocationDay(DateTimeOffset date, Guid locationId, int? cases, int? deaths, int? recoveries)
        {
            var day = Get(date, locationId);

            if (day == null)
            {
                day = new LocationDay
                {
                    Date = date,
                    Cases = cases,
                    Deaths = deaths,
                    LocationId = locationId,
                    Recoveries = recoveries
                };

                _covid19DbContext.LocationDays.Add(day);
            }

            return day;
        }

        public LocationDay Get(DateTimeOffset date, Guid locationId)
        {
            return _covid19DbContext.LocationDays.FirstOrDefault(d =>
            d.Date == date &&
            d.LocationId == locationId
            );
        }
    }
}
