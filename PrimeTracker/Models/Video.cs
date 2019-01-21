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
    public class Video : Innouvous.Utils.Merged45.MVVM45.ViewModel
    {
        public Video()
        {
            Tags = new Dictionary<TagTypes, TagRecord>();
        }

        public SolidColorBrush TitleColor
        {
            get {
                var tm = Tags;

                if (IsExpired)
                    return ColorBrushes.LightGray;
                //else if (tm.ContainsKey(TagTypes.New))
                //    return ColorBrushes.DarkOrange;
                else
                    return ColorBrushes.Black; 
            }
        }

        public string CreateDateString
        {
            get { return Created.ToShortDateString() + " " + Created.ToShortTimeString(); }
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }

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

        public Dictionary<TagTypes, TagRecord> Tags
        {
            get; set;
        }

        internal void RemoveTag(TagTypes type)
        {
            if (Tags.ContainsKey(type))
                Tags.Remove(type);
        }

        public bool IsNew
        {
            get
            {
                return Tags.ContainsKey(TagTypes.New);
            }
        }

        public bool IsExpired
        {
            get
            {
                return Tags.ContainsKey(TagTypes.Expired);
            }
        }

        public override string ToString()
        {
            return Title + " (" + AmazonId + ")";
        }
        
        internal void AddTag(TagRecord tag)
        {
            if (!Tags.ContainsKey(tag.Value))
                Tags.Add(tag.Value, tag);
            }
        }
    }