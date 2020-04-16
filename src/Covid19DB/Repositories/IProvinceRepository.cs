using Covid19DB.Entities;

namespace Covid19DB.Repositories
{
    public interface IProvinceRepository : IRepository<Province>
    {
        Province GetOrInsert(string name, Region region);
    }
}