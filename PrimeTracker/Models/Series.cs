﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeTracker.Models
{
    //[Table("tv_series")]
    class Series
    {
        public int? Id { get; set; }

        //Required
        public string Name { get; set; }

        public List<Video> Seasons { get; set; }
    }
}
