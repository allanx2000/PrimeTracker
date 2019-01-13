using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeTracker.Models
{
    public class Video
    {
        public int Id { get; set; }
        public string AmazonId { get; set; }

        public string Title { get; set; }

        public VideoType Type { get; set; }

        public double ImdbRating { get; set; }
        public double AmazonRating { get; set; }
        public int MyRating { get; set; }

        public string Url { get; set; }

        public string Description { get; set; }

        public List<TagRecord> Tags { get; set; } 

        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public DateTime? ExpiringDate { get; set; }
    }

}
