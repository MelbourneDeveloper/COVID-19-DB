using System;

namespace Covid19DB
{
    public class RawModel
    {
        public string Country_Region { get; set; }
        public string Province_State { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Long_ { get; set; }
        public int? Confirmed { get; set; }
        public int? Deaths { get; set; }
        public DateTimeOffset Date { get; set; }
        public string Admin2 { get; set; }
    }
}
