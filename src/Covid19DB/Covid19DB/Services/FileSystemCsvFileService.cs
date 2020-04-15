using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Covid19DB.Services
{
    public class FileSystemCsvFileService : ICsvFileService
    {
        #region Fields
        private readonly string _directoryPath;
        #endregion

        #region Constructor
        public FileSystemCsvFileService(string directoryPath)
        {
            _directoryPath = directoryPath;
        }
        #endregion

        #region Public Methods
        public Task<IEnumerable<string>> GetFileNamesAsync()
        {
            return Task.FromResult(Directory.GetFiles(_directoryPath, "*.csv").Select(f => new FileInfo(f).Name));
        }

        public Task<string> GetFileTextAsync(string fileName)
        {
            return File.ReadAllTextAsync(Path.Combine(_directoryPath, fileName));
        }
        #endregion
    }
}
