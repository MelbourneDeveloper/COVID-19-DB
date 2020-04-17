using Covid19DB.Db;
using Covid19DB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Covid19DB.Repositories
{
    public class RegionRepository : RepositoryBase<Region>, IRegionRepository
    {
        public RegionRepository(Covid19DbContext covid19DbContext) : base(covid19DbContext)
        {
        }

        public Region Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Insert(Region item)
        {
            Covid19DbContext.Regions.Add(item);
        }

        public Region GetOrInsert(string regionName)
        {
            var region = Get(regionName);

            if (region != null) return region;

            region = new Region { Name = regionName };

            Insert(region);

            return region;
        }

        public Region Get(string name)
        {
            return Covid19DbContext.Regions.FirstOrDefault(r => r.Name == name);
        }

#pragma warning disable CA1024 // Use properties where appropriate
        public IEnumerable<Region> GetAll()
#pragma warning restore CA1024 // Use properties where appropriate
        {
            return Covid19DbContext.Regions;
        }
    }
}
