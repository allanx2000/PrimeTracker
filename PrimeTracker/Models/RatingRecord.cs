using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeTracker.Models
{
    public class RatingRecord
    {
        public int VideoId { get; set; }

        public RatingType Type { get; set; }

        public double Value { get; set; }


        internal RatingRecord() { }
        public RatingRecord(int videoId, RatingType type, double value)
        {
            this.VideoId = videoId;
            this.Type = type;
            this.Value = value;
        }
    }
}