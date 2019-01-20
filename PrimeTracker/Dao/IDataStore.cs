﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrimeTracker.Models;

namespace PrimeTracker.Dao
{
    interface IDataStore
    {
        Video GetVideoByAmazonId(string amazonId);
        Video InsertVideo(Video v);
        void UpdateVideo(Video existing);
        List<Video> GetVideosByTag(TagTypes tag);

        /// <summary>
        /// Removes Watchlist tag for all videos not in currentIds
        /// </summary>
        /// <param name="currentIds"></param>
        void UpdateWatchlistIds(List<int> currentIds);
    }
}