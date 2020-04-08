using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Covid19DB.Model
{
    public class Province
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Fips { get; set; }
        public Guid RegionId { get; set; }
        public Region Region { get; set; }
    }
}
