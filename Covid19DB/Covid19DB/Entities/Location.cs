using System;
using System.ComponentModel.DataAnnotations;

namespace Covid19DB.Entities
{
    public class Location
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Fips { get; set; }
        public Guid ProvinceId { get; set; }
        public Province Province { get;set;}
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}
