using System;
using System.ComponentModel.DataAnnotations;

namespace Covid19DB.Entities
{
    public class Province
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Fips { get; set; }
        public Region Region { get; set; }
    }
}
