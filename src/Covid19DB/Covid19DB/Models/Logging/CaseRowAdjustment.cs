using System;

namespace Covid19DB.Models.Logging
{
    /// <summary>
    /// A figure for today is actually less than it was yesterday
    /// </summary>
    public class CaseRowAdjustment : ValidationWarningBase
    {
        public Guid LocationId { get; set; }
        public string ColumnName { get; set; }
    }
}
