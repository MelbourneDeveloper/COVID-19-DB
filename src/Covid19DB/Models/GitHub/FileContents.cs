namespace Covid19DB.Models.Github
{

    public class FileContents
    {
#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable CA1056 // Uri properties should not be strings
#pragma warning disable IDE1006 // Naming Styles
        public string name { get; set; }
        public string path { get; set; }
        public string sha { get; set; }
        public int size { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string git_url { get; set; }
        public string download_url { get; set; }
        public string type { get; set; }
        public string content { get; set; }
        public string encoding { get; set; }
        public Links _links { get; set; }
#pragma warning restore CA1707 // Identifiers should not contain underscores
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CA1056 // Uri properties should not be strings
    }
}
