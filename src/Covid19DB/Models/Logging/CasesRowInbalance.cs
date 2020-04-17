namespace Covid19DB.Models.Logging
{
    /// <summary>
    /// The figures within the row do not balance. I.e. Confirmed not equal to (Active + Deaths + Recovered)
    /// Note: Active will sometimes be zero. This is a mistake and is ignored here.
    /// </summary>
    public class CasesRowInbalance : ValidationWarningBase
    {
        public string Message { get; set; }
        public int? Confirmed { get; set; }
        public int? Deaths { get; set; }
        public int? Recoveries { get; set; }
        public int? Active { get; set; }
        public int CsvRowNumber { get; set; }

    }
}
