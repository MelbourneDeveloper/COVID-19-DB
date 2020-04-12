namespace Covid19DB.Models.Logging
{
    /// <summary>
    /// The figures within the row do not balance. I.e. Confirmed not equal to (Active + Deaths + Recovered)
    /// Note: Active will sometimes be zero. This is a mistake and is ignored here.
    /// </summary>
    public class CasesRowInbalance : ValidationWarningBase
    {

    }
}
