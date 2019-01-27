using PrimeTracker.Dao;


namespace PrimeTracker
{
    class AppContext
    {
        private static AppContext instance;

        public static Properties.Settings Settings
        {
            get { return Properties.Settings.Default; }
        }

        public static AppContext InitializeAppContext(string dataFile)
        {
            instance = new AppContext(dataFile);
            return instance;
        }

        public static AppContext Instance
        {
            get { return instance; }
        }

        private string dataFile;
        public IDataStore DataStore
        {
            get;
            private set;
        }

        private AppContext(string dataFile)
        {
            this.dataFile = dataFile;
            DataStore = new SQLiteStore(dataFile);
        }
    }
}
