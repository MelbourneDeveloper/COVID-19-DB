using System;

namespace Covid19DB.Models.Logging
{
    public class CountAnomaly
    {
        public Guid LocationId { get; set; }
        public string ColumnName { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}
