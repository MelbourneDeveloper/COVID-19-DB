using Covid19DB.Db;
using Covid19DB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Covid19DB.Repositories
{

    public class ProvinceRepository : RepositoryBase<Province>, IProvinceRepository
    {
        #region Constructor
        public ProvinceRepository(Covid19DbContext covid19DbContext) : base(covid19DbContext)
        {
        }
        #endregion

        #region Implementation
        public Province Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Insert(Province item)
        {
            Covid19DbContext.Provinces.Add(item);
        }

        public Province GetOrInsert(string name, Region region)
        {
            var province = Covid19DbContext.Provinces.FirstOrDefault(r =>
            r.Name == name &&
            r.Region.Id == region.Id
            );

            if (province != null) return province;

            province = new Province
            {
                Name = name,
                Region = region
            };
            Insert(province);

            return province;
        }

#pragma warning disable CA1024 // Use properties where appropriate
        public IEnumerable<Province> GetAll()
#pragma warning restore CA1024 // Use properties where appropriate
        {
            return Covid19DbContext.Provinces;
        }
        #endregion
    }
}
