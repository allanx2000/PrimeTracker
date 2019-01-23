using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PrimeTracker.Models
{
    [Table("videos")]
    public class Video : Innouvous.Utils.Merged45.MVVM45.ViewModel
    {
        public Video()
        {
            Tags = new Dictionary<TagType, TagRecord>();
            Ratings = new Dictionary<RatingType, RatingRecord>();
        }

        #region ViewModel

        public string CreateDateString
        {
            get { return Created.ToShortDateString() + " " + Created.ToShortTimeString(); }
        }

        public FontWeight TitleWeight
        {
            get { return IsNew ? FontWeights.Bold : FontWeights.Normal; }
        }

        public SolidColorBrush TitleColor
        {
            get
            {
                var tm = Tags;

                if (IsExpired)
                    return ColorBrushes.LightGray;
                else if (IsNew)
                    return ColorBrushes.DarkOrange;
                else
                    return ColorBrushes.Black;
            }
        }

        public bool IsNew
        {
            get
            {
                return Tags.ContainsKey(TagType.New);
            }
        }

        public bool IsExpired
        {
            get
            {
                return Tags.ContainsKey(TagType.Expired);
            }
        }

        public override void RefreshViewModel()
        {
            RaisePropertyChanged("IsExpired");
            RaisePropertyChanged("IsNew");
            RaisePropertyChanged("TitleColor");
            //TODO: RatingsString?
        }

        #endregion

        public int? Id { get; set; }

        public string AmazonId { get; set; }

        public string Title { get; set; }

        [Required]
        public VideoType Type { get; set; }

        /*
        public double ImdbRating { get; set; }
        public double AmazonRating { get; set; }
        public int MyRating { get; set; }
        */

        [Required]
        public string Url { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [Required]
        public DateTime Updated { get; set; }

        public Dictionary<TagType, TagRecord> Tags
        {
            get; set;
        }
        public Dictionary<RatingType, RatingRecord> Ratings { get; internal set; }

        internal void RemoveTag(TagType type)
        {
            if (Tags.ContainsKey(type))
            {
                Tags.Remove(type);
                RefreshViewModel();
            }
        }

        public override string ToString()
        {
            return Title + " (" + AmazonId + ")";
        }

        internal void AddRating(RatingRecord rating)
        {
            if (!Ratings.ContainsKey(rating.Type))
            {
                Ratings.Add(rating.Type, rating);
                RefreshViewModel();
            }
        }

        internal void RemoveRating(RatingType type)
        {
            if (Ratings.ContainsKey(type))
            {
                Ratings.Remove(type);
                RefreshViewModel();
            }
        }

        internal void AddTag(TagRecord tag)
        {
            if (!Tags.ContainsKey(tag.Value))
            {
                Tags.Add(tag.Value, tag);
                RefreshViewModel();
            }
        }
    }
}