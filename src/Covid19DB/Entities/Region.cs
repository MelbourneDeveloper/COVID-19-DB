using System;
using System.ComponentModel.DataAnnotations;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Covid19DB.Entities
{
    public class Region
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
