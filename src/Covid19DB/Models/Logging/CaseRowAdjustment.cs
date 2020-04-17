namespace Covid19DB.Models.Logging
{
    /// <summary>
    /// A figure for today is actually less than it was yesterday
    /// </summary>
    public class CaseRowAdjustment : ValidationWarningBase
    {
        public string Region { get; set; }
        public string Provice { get; set; }
        public string Location { get; set; }
        public string ColumnName { get; set; }
        public int Discrepancy { get; set; }
        public string PreviousDayRowUrl { get; set; }
    }
}
