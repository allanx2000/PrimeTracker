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
using System.Threading.Tasks;
using System.Diagnostics;

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

            IsRefreshing = true;

            InitializeWatchlist();
            InitializeRecentlyAdded();

            LoadFromContext();

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

        internal void OpenVideo(Video video)
        {
            if (video != null && !string.IsNullOrEmpty(video.Url))
            {
                Process.Start(video.Url);

                video.RemoveTag(TagType.New);
                AppContext.Instance.DataStore.UpdateVideo(video);
            }
        }

        private IDataStore DataStore
        {
            get
            {
                return AppContext.Instance.DataStore;
            }
        }

        #region Recently Added

        private RefreshResults RefreshRecentlyAdded()
        {
            var list = Browser.GetRecentlyAddedVideos();

            List<Video> added = new List<Video>();
            List<Video> duplicates = new List<Video>();
            List<Video> failedIds = new List<Video>();

            DateTime runTime = DateTime.Now;


            foreach (Video v in list)
            {
                var existing = DataStore.GetExistingVideo(v.Type, v.AmazonId, v.Title);

                if (existing == null)
                {
                    v.Created = v.Updated = runTime;
                    v.AddTag(TagRecord.Create(-1, TagType.New));

                    if ((from i in added
                         where i.AmazonId == v.AmazonId || i.Title == v.Title
                         select i).FirstOrDefault() == null)
                    {
                        try
                        {
                            DataStore.InsertVideo(v);
                            added.Add(v);
                        }
                        catch (Exception e)
                        {
                            failedIds.Add(v);
                        }
                    }
                    else
                    {
                        duplicates.Add(v);
                    }
                }
                else
                {
                    existing.Updated = runTime;
                    DataStore.UpdateVideo(existing);
                }
            }

            RefreshResults result = new RefreshResults(added, failedIds);
            return result;
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

            foreach (var video in DataStore.GetVideosByCreatedDate(30))
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

        public ICommand RefreshRecentCommand
        {
            get
            {
                return new CommandHelper(RefreshRecentMoviesAsync);
            }
        }

        private async void RefreshRecentMoviesAsync()
        {
            IsRefreshing = false;

            await Task.Run(() =>
            {
                try
                {
                    var result = RefreshRecentlyAdded();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        ShowRefreshStatus(result);
                        LoadFromContext();
                        IsRefreshing = true;
                    });
                }
                catch (Exception e)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBoxFactory.ShowError(e);
                        IsRefreshing = true;
                    });
                }
            });
        }

        private void ShowRefreshStatus(RefreshResults result, string title = "Refreshed")
        {
            MessageBoxFactory.ShowInfo(mainWindow, result.GetSummary(), title);
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
                return ((Video)obj).IsExpired;
            else
                return true;
        }

        public bool IsRefreshing
        {
            get { return Get<bool>(); }
            set
            {
                Set(value);
                RaisePropertyChanged();
            }
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

            foreach (var video in DataStore.GetVideosByTag(TagType.WatchList))
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
                return new CommandHelper(RefreshWatchlistItemsAsync);
            }
        }

        private async void RefreshWatchlistItemsAsync()
        {
            IsRefreshing = false;

            await Task.Run(() =>
            {
                try
                {
                //TODO: Disable Button
                RefreshWatchlistItems(false);

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        LoadFromContext();
                        IsRefreshing = true;
                    });
                }
                catch (Exception e)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBoxFactory.ShowError(e);
                        IsRefreshing = true;
                    });
                }
            });
        }

        public void RefreshWatchlistItems(bool reload)
        {
            List<int> currentIds = new List<int>();

            foreach (var i in Browser.GetWatchListVideos())
            {
                var existing = DataStore.GetVideoByAmazonId(i.AmazonId);
                int id;

                if (existing != null)
                {

                    id = existing.Id.Value;

                    SetWatchlistValues(existing, i);
                    id = existing.Id.Value;
                    DataStore.UpdateVideo(existing);
                }
                else
                {
                    SetWatchlistValues(i);
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
                    SetWatchlistValues(existing, i);
                    id = existing.Id.Value;
                    DataStore.UpdateVideo(existing);
                }
                else
                {
                    SetWatchlistValues(i);
                    var video = DataStore.InsertVideo(i);
                    id = video.Id.Value;
                }

                currentIds.Add(id);
            }

            DataStore.UpdateWatchlistIds(currentIds);
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

        private static void SetWatchlistValues(Video originai, Video updated = null)
        {
            DateTime runTime = DateTime.Now;


            var watchListTag = TagRecord.Create(originai.Id.HasValue ? originai.Id.Value : -1, TagType.WatchList);


            if (updated == null)
            {
                originai.Created = originai.Updated = runTime;
                originai.AddTag(watchListTag);
            }
            else
            {
                originai.Updated = runTime;

                originai.AddTag(watchListTag);

                if (updated.Tags != null)
                {
                    foreach (var t in updated.Tags.Values)
                    {
                        originai.AddTag(t);
                    }
                }

            }
        }
    }
}