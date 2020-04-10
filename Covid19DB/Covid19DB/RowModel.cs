using System;
// ReSharper disable InconsistentNaming

namespace Covid19DB
{
    public class RowModel
    {
#pragma warning disable CA1707 // Identifiers should not contain underscores
        public string Country_Region { get; set; }
        public string Province_State { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Long_ { get; set; }
        public int? Confirmed { get; set; }
        public int? Deaths { get; set; }
        public int? Recovered { get; set; }
        public DateTimeOffset Date { get; set; }
        public string Admin2 { get; set; }
#pragma warning restore CA1707 // Identifiers should not contain underscores
    }
}
