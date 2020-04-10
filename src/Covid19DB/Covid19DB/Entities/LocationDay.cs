﻿using System;
using System.ComponentModel.DataAnnotations;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Covid19DB.Entities
{
    public class LocationDay
    {
        [Key]
        public Guid Id { get; set; }
        public DateTimeOffset DateOfCount { get; set; }
        public int? NewCases { get; set; }
        public int? Deaths { get; set; }
        public int? Recoveries { get; set; }
        public Location Location { get; set; }
    }
}
