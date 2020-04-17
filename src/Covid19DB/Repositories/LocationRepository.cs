using Covid19DB.Db;
using Covid19DB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Covid19DB.Repositories
{
    public class LocationRepository : RepositoryBase<Location>, ILocationRepository
    {
        public LocationRepository(Covid19DbContext covid19DbContext) : base(covid19DbContext)
        {
        }

        public Location Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Insert(Location item)
        {
            Covid19DbContext.Locations.Add(item);
        }

        public Location GetOrInsert(string locationName, Province province, decimal? latitude, decimal? longitude)
        {
            var location = Covid19DbContext.Locations.FirstOrDefault(l =>
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
            Covid19DbContext.Locations.Add(location);

            return location;
        }

#pragma warning disable CA1024 // Use properties where appropriate
        public IEnumerable<Location> GetAll()
#pragma warning restore CA1024 // Use properties where appropriate
        {
            return Covid19DbContext.Locations;
        }


    }
}
