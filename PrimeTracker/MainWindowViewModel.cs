using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using PrimeTracker.Browsers;
using PrimeTracker.Models;
using Innouvous.Utils.MVVM;
using System.Windows.Input;

namespace PrimeTracker
{
    internal class MainWindowViewModel : Innouvous.Utils.Merged45.MVVM45.ViewModel
    {
        private MainWindow mainWindow;
        private readonly PrimeBrowser browser;


        public MainWindowViewModel(MainWindow mainWindow)
        {
            browser = new PrimeBrowser();

            this.mainWindow = mainWindow;

            InitializeWatchlist();
        }


        #region Watchlist

        private CollectionViewSource cvsMovies;
        private ObservableCollection<Video> movies = new ObservableCollection<Video>();
        private ObservableCollection<Video> shows = new ObservableCollection<Video>();
        private CollectionViewSource cvsShows;

        private void InitializeWatchlist()
        {
            SortDescription SortTitle = new SortDescription("Title", ListSortDirection.Ascending);

            cvsMovies = new CollectionViewSource
            {
                Source = movies
            };
            cvsMovies.SortDescriptions.Add(SortTitle);
            cvsMovies.View.Filter = FilterVideos;

            cvsShows = new CollectionViewSource
            {
                Source = shows
            };

            cvsShows.SortDescriptions.Add(SortTitle);
            cvsShows.View.Filter = FilterVideos;

            LoadFromContext();
        }

        /// <summary>
        /// If true, show the video
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool FilterVideos(object obj)
        {
            if (ExpiredOnly)
                return ((Video)obj).ExpiringDate != null;
            else
                return true;
        }

        public bool ExpiredOnly
        {
            get { return Get<bool>(); }
            set
            {
                Set(value);
                RaisePropertyChanged();
                cvsShows.View.Refresh();
                cvsMovies.View.Refresh();
            }
        }

        internal void CloseBrowser()
        {
            if (browser != null)
                browser.Quit();
        }


        //TODO: Refactor out and use Settings
        private void LoadFromContext()
        {
            if (AppContext.Instance == null)
            {
                AppContext.InitializeAppContext("E:\\test.db");
            }

            LoadWatchlist();
        }

        private void LoadWatchlist()
        {
            shows.Clear();
            movies.Clear();

            foreach (var tag in AppContext.Instance.allTags.Where(t => t.Value == TagTypes.WatchList))
            {

                switch (tag.Parent.Type)
                {
                    case VideoType.Movie:
                        movies.Add(tag.Parent);
                        break;
                    case VideoType.TvSeason:
                        shows.Add(tag.Parent);
                        break;
                }
            }
        }

        public ICollectionView Movies
        {
            get { return cvsMovies.View; }
        }

        public ICollectionView Shows
        {
            get { return cvsShows.View; }
        }

        public ICommand RefreshWatchlistCommand
        {
            get
            {
                return new CommandHelper(RefreshWatchlistItems);
            }
        }

        public void RefreshWatchlistItems()
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

        #endregion

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