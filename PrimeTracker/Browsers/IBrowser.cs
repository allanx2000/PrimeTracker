using PrimeTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeTracker.Browsers
{
    public interface IBrowser
    {
        List<Video> GetRecentlyAddedVideos();
        List<Video> GetWatchListVideos(VideoType type = VideoType.Movie);
        Video GetDetailsForVideo(Video dbRecord);
    }
}
