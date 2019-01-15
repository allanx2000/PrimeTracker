using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using PrimeTracker.Browsers;
using PrimeTracker.Models;

namespace PrimeTracker
{
    internal class MainWindowViewModel : Innouvous.Utils.Merged45.MVVM45.ViewModel
    {
        private MainWindow mainWindow;
        private CollectionViewSource cvsExpiredMovies;
        private ObservableCollection<Video> movies = new ObservableCollection<Video>();
        private ObservableCollection<Video> shows = new ObservableCollection<Video>();
        private CollectionViewSource cvsExpiredShows;
        private readonly PrimeBrowser browser;

        public ICollection<Video> AllMovies
        {
            get { return movies; }
        }

        public ICollection<Video> AllShows
        {
            get { return shows; }
        }

        public MainWindowViewModel(MainWindow mainWindow)
        {
            browser = new PrimeBrowser();

            this.mainWindow = mainWindow;

            SortDescription SortTitle = new SortDescription("Title", ListSortDirection.Ascending);

            cvsExpiredMovies = new CollectionViewSource();
            cvsExpiredMovies.Source = movies;
            cvsExpiredMovies.SortDescriptions.Add(SortTitle);
            cvsExpiredMovies.View.Filter = (x) => ((Video)x).ExpiringDate != null;

            cvsExpiredShows = new CollectionViewSource();
            cvsExpiredShows.Source = shows;
            cvsExpiredShows.SortDescriptions.Add(SortTitle);
            cvsExpiredShows.View.Filter = (x) => ((Video)x).ExpiringDate != null;

            LoadFromContext();
        }

        internal void CloseBrowser()
        {
            if (browser != null)
                browser.Quit();
        }

        private void LoadFromContext()
        {
            if (AppContext.Instance == null)
            {
                AppContext.InitializeAppContext("E:\\test.db");
            }

            AllMovies.Clear();
            AllShows.Clear();

            foreach (var v in AppContext.Instance.allVideos)
            {
                switch (v.Type)
                {
                    case VideoType.Movie:
                        AllMovies.Add(v);
                        break;
                    case VideoType.TvSeason:
                        AllShows.Add(v);
                        break;
                }
            }
        }

        public ICollectionView ExpiredMovies
        {
            get { return cvsExpiredMovies.View; }
        }

        public ICollectionView ExpiredShows
        {
            get { return cvsExpiredShows.View; }
        }

        public void RefreshWatchListItems()
        {
            foreach (var i in browser.GetWatchListVideos())
            {
                var existing = (from x in AppContext.Instance.allVideos
                                where x.AmazonId == i.AmazonId
                                select x).FirstOrDefault();

                if (existing != null)
                {
                    SetValues(existing, i);
                }
                else
                {
                    SetValues(i);
                    AppContext.Instance.allVideos.Add(i);
                }
            }

            foreach (var i in browser.GetWatchListVideos(VideoType.TvSeason))
            {
                var existing = (from x in AppContext.Instance.allVideos
                                where x.AmazonId == i.AmazonId
                                select x).FirstOrDefault();

                if (existing != null)
                {
                    SetValues(existing, i);
                }
                else
                {
                    SetValues(i);
                    AppContext.Instance.allVideos.Add(i);
                }
            }

            LoadFromContext();
        }

        private static void SetValues(Video originai, Video updated = null)
        {
            if (updated == null)
            {
                originai.Created = originai.Updated = DateTime.Now;

                originai.Tags = new List<TagRecord>(); //Is this null?
                originai.Tags.Add(TagRecord.Create(TagTypes.WatchList));

                if (originai.ExpiringDate != null)
                    originai.Tags.Add(TagRecord.Create(TagTypes.Expired));
            }
            else
            {
                originai.Updated = DateTime.Now;

                var tags = originai.TagMap;

                if (originai.ExpiringDate != null && tags.ContainsKey(TagTypes.Expired))
                {
                    originai.Tags.Add(TagRecord.Create(TagTypes.Expired));
                }
            }
        }
    }
}