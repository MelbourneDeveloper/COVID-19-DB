using System;

namespace Covid19DB.Models.Logging
{
    public class CountAnomaly : ValidationWarningBase
    {
        public Guid LocationId { get; set; }
        public string ColumnName { get; set; }
    }
}
