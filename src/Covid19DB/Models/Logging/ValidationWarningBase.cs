using System;

namespace Covid19DB.Models.Logging
{
    public class ValidationWarningBase
    {
        public DateTimeOffset Date { get; set; }
        public int CsvRowNumber { get; set; }
        public string Url { get; set; }
    }
}
