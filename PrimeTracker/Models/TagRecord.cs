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

        public virtual Video Parent { get; set; }

        private TagRecord() { }

        internal static TagRecord Create(TagTypes value)
        {
            TagRecord tr = new TagRecord()
            {
                Added = DateTime.Now,
                Value = value
            };

            return tr;
        }
    }
}