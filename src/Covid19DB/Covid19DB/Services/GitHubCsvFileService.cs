
/*
using Covid19DB.Models.Github;
using RestClient.Net;
using RestClient.Net.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Covid19DB.Services
{
    public class GitHubCsvFileService : ICsvFileService
    {
        #region Fields
        private Client Client { get; } = new Client(new NewtonsoftSerializationAdapter(), new Uri("https://api.github.com/repos/CSSEGISandData/COVID-19/"));
        #endregion

        #region Constructor
        public GitHubCsvFileService()
        {
            Client.DefaultRequestHeaders.Add("User-Agent", "curl/7.55.1");
        }
        #endregion

        #region Implementation 
        public async Task<IEnumerable<string>> GetFileNamesAsync()
        {
            var fileNames = new List<string>();

            var response = await Client.GetAsync<List<Contents>>("contents/Pluralsight");

            foreach (var contents in response.Body)
            {
                if (contents.type == ContentType.File && contents.name.Contains(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    fileNames.Add(contents.name);
                }
            }

            return fileNames;
        }

        public Task<string> GetFileTextAsync(string fileName)
        {
            var length = Client.BaseUri.ToString().Length;
            var resource = contents.url.Substring(length, contents.url.Length - length);

            var file = Client.GetAsync<FileContents>(resource);
            var data = Convert.FromBase64String(file.Body.content);
        }
        #endregion

    }
}

    */