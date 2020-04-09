using Covid19DB.Entities;
using System;
using System.Linq;

namespace Covid19DB.Repositories
{
    public class ProvinceRepository : IRepository<Province>, IProvinceRepository
    {
        #region Fields
        private Covid19DbContext _Covid19DbContext;
        #endregion

        #region Constructor
        public ProvinceRepository(Covid19DbContext covid19DbContext)
        {
            _Covid19DbContext = covid19DbContext;
        }
        #endregion

        #region Implementation
        public Province Get(Guid Id)
        {
            throw new NotImplementedException();
        }

        public void Insert(Province item)
        {
            _Covid19DbContext.Provinces.Add(item);
        }

        public Province GetOrInsert(string name, Guid regionId)
        {
            var province = _Covid19DbContext.Provinces.FirstOrDefault(r =>
            r.Name == name &&
            r.RegionId == regionId
            );

            if (province == null)
            {
                province = new Province
                {
                    Name = name,
                    RegionId = regionId
                };
                Insert(province);
            }

            return province;
        }
        #endregion
    }
}
