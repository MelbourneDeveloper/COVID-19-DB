using System.Collections.Generic;
using System.IO;

namespace Covid19DB.Services
{
    public interface ICsvFileService
    {
        IEnumerable<string> GetFileNames();
        Stream OpenStream(string fileName);
    }
}