using System;

namespace Covid19DB
{
    public class RollingTotalsInfo
    {
        public int? RollingTotal { get; set; }
        public DateTimeOffset PreviousDate { get; set; }
        public int PreviousDateRowNumber { get; set; }
    }
}
