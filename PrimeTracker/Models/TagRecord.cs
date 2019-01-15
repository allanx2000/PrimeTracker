using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeTracker.Models
{

    [Table("tags")]
    public class TagRecord
    {
        [Key, ForeignKey("Parent"), Column(Order = 0)]
        public int VideoId { get; set; }

        [Key, Column(Order = 1)]
        public TagTypes Value { get; set; }

        [Required]
        public DateTime Added { get; set; }

        public Video Parent { get; set; }

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