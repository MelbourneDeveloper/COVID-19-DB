using System;
using System.Collections.Generic;
using System.Text;

namespace Covid19DB.Model
{
    public class Day
    {
        public Guid Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public int Cases { get; set; }
        public int Deaths { get; set; }
        public Guid LocationId { get; set; }
    }
}
