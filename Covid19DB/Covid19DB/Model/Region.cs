using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Covid19DB.Model
{
    public class Region
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
