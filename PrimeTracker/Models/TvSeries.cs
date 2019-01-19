using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeTracker.Models
{
    [Table("tv_series")]
    class TvSeries
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }

        [Required, Index("IX_SeriesName", IsUnique = true)]
        public string Name { get; set; }

        public List<Video> Seasons { get; set; }
    }
}
