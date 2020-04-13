﻿using System;

namespace Covid19DB.Models.Logging
{
    public class ValidationWarningBase
    {
        public DateTimeOffset Date { get; set; }
        public int CsvRowNumber { get; set; }
        public int? Confirmed { get; set; }
        public int? Deaths { get; set; }
        public int? Recoveries { get; set; }
        public int? Active { get; set; }
        public string Url { get; set; }
        public string Message { get; set; }
    }
}