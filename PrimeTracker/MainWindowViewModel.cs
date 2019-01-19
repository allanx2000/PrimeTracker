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
using Innouvous.Utils;
using PrimeTracker.Dao;

namespace PrimeTracker
{
    internal class MainWindowViewModel : Innouvous.Utils.Merged45.MVVM45.ViewModel
    {
        private MainWindow mainWindow;
        private PrimeBrowser browser;

        private PrimeBrowser Browser
        {
            get
            {
                if (browser == null)
                    browser = new PrimeBrowser();

                return browser;
            }
        }

        public MainWindowViewModel(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            InitializeWatchlist();
            InitializeRecentlyAdded();

            LoadFromContext();

            //RefreshRecentlyAdded();
        }



        //TODO: Refactor out and use Settings
        private void LoadFromContext()
        {
            var ctx = AppContext.InitializeAppContext(AppContext.Settings.DbPath);

            /*
            var today = (from i in AppContext.Instance.allVideos where i.Created >= DateTime.Today select i).ToList();

            AppContext.Instance.allVideos.RemoveRange(today);
            AppContext.Instance.SaveChanges();
            */

            LoadWatchlist();
            LoadRecentlyAdded();
        }

        private IDataStore DataStore
        {
            get
            {
                return AppContext.Instance.DataStore;
            }
        }

        #region Recently Added

        private void RefreshRecentlyAdded()
        {
            var list = Browser.GetRecentlyAddedVideos();

            List<Video> added = new List<Video>();
            List<Video> duplicates = new List<Video>();


            foreach (Video v in list)
            {
                var existing = DataStore.GetVideoByAmazonId(v.AmazonId);

                if (existing == null)
                {
                    v.Created = v.Updated = DateTime.Today;
                    v.Tags = new List<TagRecord>();
                    v.Tags.Add(TagRecord.Create(TagTypes.New));

                    if ((from i in added
                         where i.AmazonId == v.AmazonId || i.Title == v.Title
                         select i).FirstOrDefault() == null)
                    {
                        DataStore.InsertVideo(v);
                        added.Add(v);
                    }
                    else
                    {
                        duplicates.Add(v);
                    }
                }
                else
                {
                    existing.Updated = DateTime.Today;
                    DataStore.UpdateVideo(existing);
                }
            }
        }

        //RecentlyAddedMovies
        private CollectionViewSource cvsRecentMovies;
        private ObservableCollection<Video> recentMovies = new ObservableCollection<Video>();

        public ICollectionView RecentlyAddedMovies
        {
            get { return cvsRecentMovies.View; }
        }


        private void InitializeRecentlyAdded()
        {
            SortDescription SortTitle = new SortDescription("Title", ListSortDirection.Ascending);
            SortDescription SortCreated = new SortDescription("Created", ListSortDirection.Descending);

            cvsRecentMovies = new CollectionViewSource
            {
                Source = recentMovies
            };
            cvsRecentMovies.SortDescriptions.Add(SortCreated);
            cvsRecentMovies.SortDescriptions.Add(SortTitle);
        }

        private void LoadRecentlyAdded()
        {
            recentMovies.Clear();

            //TODO: Need to add expire new?

            foreach (var video in DataStore.GetVideosByTag(TagTypes.New))
            {

                switch (video.Type)
                {
                    case VideoType.Movie:
                        recentMovies.Add(video);
                        break;
                    case VideoType.TvSeason:
                        //recentShows.Add(tag.Parent);
                        break;
                }
            }
        }


        #endregion

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

        private void LoadWatchlist()
        {
            shows.Clear();
            movies.Clear();

            foreach (var video in DataStore.GetVideosByTag(TagTypes.WatchList))
            {

                switch (video.Type)
                {
                    case VideoType.Movie:
                        movies.Add(video);
                        break;
                    case VideoType.TvSeason:
                        shows.Add(video);
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
            List<int> currentIds = new List<int>();

            foreach (var i in Browser.GetWatchListVideos())
            {
                var existing = DataStore.GetVideoByAmazonId(i.AmazonId);
                int id;

                if (existing != null)
                {

                    id = existing.Id.Value;

                    SetValues(existing, i);
                    id = existing.Id.Value;
                    DataStore.UpdateVideo(i);
                }
                else
                {
                    SetValues(i);
                    var video = DataStore.InsertVideo(i);
                    id = video.Id.Value;
                }

                currentIds.Add(id);
            }

            foreach (var i in Browser.GetWatchListVideos(VideoType.TvSeason))
            {
                var existing = DataStore.GetVideoByAmazonId(i.AmazonId);
                int id;

                if (existing != null)
                {
                    SetValues(existing, i);
                    id = existing.Id.Value;
                    DataStore.UpdateVideo(i);
                }
                else
                {
                    SetValues(i);
                    var video = DataStore.InsertVideo(i);
                    id = video.Id.Value;
                }

                currentIds.Add(id);
            }

            DataStore.UpdateWatchlistIds(currentIds);

            LoadFromContext();
        }

        #endregion

        public ICommand ShowSettingsCommand
        {
            get
            {
                return new CommandHelper(ShowSettings);
            }
        }

        private void ShowSettings()
        {
            var dlg = new SettingsWindow();
            dlg.Owner = mainWindow;
            dlg.ShowDialog();

            if (dlg.Changed)
            {
                mainWindow.Reload();
            }
        }

        private static void SetValues(Video originai, Video updated = null)
        {
            if (updated == null)
            {
                originai.Created = originai.Updated = DateTime.Today;

                originai.Tags = new List<TagRecord>(); //Is this null?
                originai.Tags.Add(TagRecord.Create(TagTypes.WatchList));

                if (originai.ExpiringDate != null)
                    originai.Tags.Add(TagRecord.Create(TagTypes.Expired));
            }
            else
            {
                originai.Updated = DateTime.Today;

                var tags = originai.TagMap;

                if (originai.ExpiringDate != null && tags.ContainsKey(TagTypes.Expired))
                {
                    originai.Tags.Add(TagRecord.Create(TagTypes.Expired));
                }
            }
        }
    }
}