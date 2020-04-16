using Covid19DB.Db;
using Covid19DB.Entities;
using System;
using System.Linq;

namespace Covid19DB.Repositories
{
    public class ProvinceRepository : IProvinceRepository
    {
        #region Fields
        private readonly Covid19DbContext _covid19DbContext;
        #endregion

        #region Constructor
        public ProvinceRepository(Covid19DbContext covid19DbContext)
        {
            _covid19DbContext = covid19DbContext;
        }
        #endregion

        #region Implementation
        public Province Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Insert(Province item)
        {
            _covid19DbContext.Provinces.Add(item);
        }

        public Province GetOrInsert(string name, Region region)
        {
            var province = _covid19DbContext.Provinces.FirstOrDefault(r =>
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
        #endregion
    }
}
