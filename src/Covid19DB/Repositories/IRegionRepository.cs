using Covid19DB.Entities;

namespace Covid19DB.Repositories
{
    public interface IRegionRepository : IRepository<Region>
    {
        Region Get(string name);
        Region GetOrInsert(string regionName);
    }
}