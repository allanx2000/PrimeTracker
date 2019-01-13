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

            Refresh();
        }

        public ICollectionView ExpiredMovies
        {
            get { return cvsExpiredMovies.View; }
        }

        public ICollectionView ExpiredShows
        {
            get { return cvsExpiredShows.View; }
        }

        public void Refresh()
        {
            movies.Clear();
            
            foreach (var i in browser.GetWatchListVideos())
            {
                movies.Add(i);
            }

            shows.Clear();
            foreach (var i in browser.GetWatchListVideos(VideoType.TvSeason))
            {
                shows.Add(i);
            }
        }
    }
}