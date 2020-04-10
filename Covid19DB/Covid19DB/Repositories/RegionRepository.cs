using Covid19DB.Db;
using Covid19DB.Entities;
using System;
using System.Linq;

namespace Covid19DB.Repositories
{
    public class RegionRepository : IRepository<Region>, IRegionRepository
    {
        private readonly Covid19DbContext _covid19DbContext;

        public RegionRepository(Covid19DbContext covid19DbContext)
        {
            _covid19DbContext = covid19DbContext;
        }

        public Region Get(Guid Id)
        {
            throw new NotImplementedException();
        }

        public void Insert(Region item)
        {
            _covid19DbContext.Regions.Add(item);
        }

        public Region GetOrInsert(string regionName)
        {
            var region = Get(regionName);
            if (region == null)
            {
                region = new Region { Name = regionName };
                Insert(region);
            }

            return region;
        }

        public Region Get(string name)
        {
            return _covid19DbContext.Regions.FirstOrDefault(r => r.Name == name);
        }
    }
}
