using System.Collections.Generic;
using System.Threading.Tasks;

namespace Covid19DB.Services
{
    public interface ICsvFileService
    {
        Task<IEnumerable<string>> GetFileNamesAsync();
        Task<string> GetFileTextAsync(string fileName);
    }
}