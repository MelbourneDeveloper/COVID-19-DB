using Covid19DB.Entities;

namespace Covid19DB.Repositories
{
    public interface ILocationRepository : IRepository<Location>
    {
        Location GetOrInsert(string locationName, Province province, decimal? latitude, decimal? longitude);
    }
}