using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeTracker.Models
{
    public class TagRecord
    {
        public int VideoId { get; set; }

        public TagTypes Value { get; set; }

        public DateTime Added { get; set; }

        internal static TagRecord Create(int videoId, TagTypes value)
        {
            TagRecord tr = new TagRecord()
            {
                VideoId = videoId,
                Added = DateTime.Now,
                Value = value
            };

            return tr;
        }
    }
}