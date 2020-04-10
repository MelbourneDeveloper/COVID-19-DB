using System;
using System.ComponentModel.DataAnnotations;

namespace Covid19DB.Entities
{
    public class LocationDay
    {
        [Key]
        public Guid Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public int? Cases { get; set; }
        public int? Deaths { get; set; }
        public int? Recoveries { get; set; }
        public Guid LocationId { get; set; }
        public Location Location { get; set; }
    }
}
