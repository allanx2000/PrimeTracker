using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeTracker.Models
{
    public class TagRecord
    {
        public int VideoId { get; set; }

        public TagType Value { get; set; }

        //TODO: Remove, not needed since new one will always bet inserted
        public DateTime Added { get; set; }

        internal static TagRecord Create(int videoId, TagType value)
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