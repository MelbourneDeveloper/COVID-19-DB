using Covid19DB.Db;
using Covid19DB.Entities;
using System;
using System.Linq;

namespace Covid19DB.Repositories
{
    public class LocationRepository : IRepository<Location>, ILocationRepository
    {
        private readonly Covid19DbContext _covid19DbContext;

        public LocationRepository(Covid19DbContext covid19DbContext)
        {
            _covid19DbContext = covid19DbContext;
        }

        public Location Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Insert(Location item)
        {
            _covid19DbContext.Locations.Add(item);
        }

        public Location GetOrInsert(string locationName, Province province, decimal? latitude, decimal? longitude)
        {
            var location = _covid19DbContext.Locations.FirstOrDefault(l =>
            l.Name == locationName &&
            l.Province.Id == province.Id
            );

            if (location != null) return location;

            location = new Location
            {
                Name = locationName,
                Province = province,
                Latitude = latitude,
                Longitude = longitude
            };
            _covid19DbContext.Locations.Add(location);

            return location;
        }
    }
}
