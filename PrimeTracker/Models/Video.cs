using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PrimeTracker.Models
{
    [Table("videos")]
    public class Video
    {
        
        public SolidColorBrush TitleColor
        {
            get { return ExpiringDate == null ? ColorBrushes.Black : ColorBrushes.LightGray; }
        }


        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, Index("IX_AmazonId", IsUnique = true)]
        public string AmazonId { get; set; }

        [Required, Index("IX_VideoName", IsUnique = true)]
        public string Title { get; set; }

        [Required]
        public VideoType Type { get; set; }

        public double ImdbRating { get; set; }
        public double AmazonRating { get; set; }
        public int MyRating { get; set; }

        [Required]
        public string Url { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [Required]
        public DateTime Updated { get; set; }

        public DateTime? ExpiringDate { get; set; }

        public virtual ICollection<TagRecord> Tags { get; set; }

        public Dictionary<TagTypes, TagRecord> TagMap
        {
            get
            {
                var dict = new Dictionary<TagTypes, TagRecord>();

                foreach (var t in Tags)
                {
                    dict[t.Value] = t;
                }

                return dict;
            }
        }
    }

}
