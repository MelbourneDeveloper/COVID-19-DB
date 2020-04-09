using Covid19DB.Entities;
using System;
using System.Linq;

namespace Covid19DB.Repositories
{
    public class LocationRepository : IRepository<Location>, ILocationRepository
    {
        private Covid19DbContext _covid19DbContext;

        public LocationRepository(Covid19DbContext covid19DbContext)
        {
            _covid19DbContext = covid19DbContext;
        }

        public Location Get(Guid Id)
        {
            throw new NotImplementedException();
        }

        public void Insert(Location item)
        {
            _covid19DbContext.Locations.Add(item);
        }

        public Location GetOrInsert(string locationName, Guid provinceId, decimal? latitude, decimal? longitude)
        {
            var location = _covid19DbContext.Locations.FirstOrDefault(l =>
            l.Name == locationName &&
            l.ProvinceId == provinceId
            );

            if (location == null)
            {
                location = new Location
                {
                    Name = locationName,
                    ProvinceId = provinceId,
                    Latitude = latitude,
                    Longitude = longitude
                };
                _covid19DbContext.Locations.Add(location);
            }

            return location;
        }
    }
}
