using System;

namespace Covid19DB.Entities
{
    public class LocationDay
    {
        public Guid Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public int? Cases { get; set; }
        public int? Deaths { get; set; }
        public int? Recoveries { get; set; }
        public Guid LocationId { get; set; }
    }
}
