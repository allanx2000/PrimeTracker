using System;

namespace PrimeTracker.Models
{
    public class TagRecord
    {
        public int VideoId { get; set; }

        public TagTypes Value { get; set; }

        public DateTime Created { get; set; }
    }
}