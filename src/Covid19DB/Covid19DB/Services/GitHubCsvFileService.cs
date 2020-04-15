
using Covid19DB.Models.Github;
using RestClient.Net;
using RestClient.Net.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Covid19DB.Services
{
    public class GitHubCsvFileService : ICsvFileService
    {
        #region Fields
        private Client _client { get; } = new Client(new NewtonsoftSerializationAdapter());
        private Dictionary<string, string> _fileUrlsByName;
        #endregion

        #region Constructor
        public GitHubCsvFileService()
        {
            _client.DefaultRequestHeaders.Add("User-Agent", "curl/7.55.1");
        }
        #endregion


        #region Implementation 
        public async Task<IEnumerable<string>> GetFileNamesAsync()
        {
            var fileNames = new List<string>();
            var response = await _client.GetAsync<List<Contents>>(new Uri("https://api.github.com/repos/CSSEGISandData/COVID-19/contents/csse_covid_19_data/csse_covid_19_daily_reports?ref=master"));

            _fileUrlsByName = new Dictionary<string, string>();

            foreach (var contents in response.Body)
            {
                if (contents.type == ContentType.File && contents.name.Contains(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    _fileUrlsByName.Add(contents.name, contents.url);
                    fileNames.Add(contents.name);
                }
            }

            return fileNames;
        }

        public async Task<string> GetFileTextAsync(string fileName)
        {
            if (_fileUrlsByName == null) throw new ArgumentException($"{nameof(GetFileNamesAsync)} must be called before {nameof(GetFileTextAsync)}");

            var resource = new Uri(_fileUrlsByName[fileName]);
            var file = await _client.GetAsync<FileContents>(resource);
            var data = Convert.FromBase64String(file.Body.content);
            var text = Encoding.ASCII.GetString(data);
#pragma warning disable CA1307
            //Remove the dodgy characters at the start
            var cleanedText = text.Replace("???", string.Empty);
#pragma warning restore CA1307 
            return cleanedText;
        }
        #endregion

    }
}