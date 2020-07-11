using System;
using System.ComponentModel.DataAnnotations;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Covid19DB.Entities
{
    public class Location
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Fips { get; set; }
        public Province Province { get;set;}
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}
