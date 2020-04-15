using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        public IEnumerable<string> GetFileNames()
        {
            return Directory.GetFiles(_directoryPath, "*.csv").Select(f => new FileInfo(f).Name);
        }

        public Stream OpenStream(string fileName)
        {
            return File.OpenRead(Path.Combine(_directoryPath, fileName));
        }
        #endregion
    }
}
