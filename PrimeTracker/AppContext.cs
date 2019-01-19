using PrimeTracker.Dao;
using PrimeTracker.Models;
using SQLite.CodeFirst;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Data.SQLite;
using System.Data.SQLite.EF6;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PrimeTracker
{
    public class SQLiteConfiguration : DbConfiguration
    {
        public SQLiteConfiguration()
        {
            SetProviderFactory("System.Data.SQLite", SQLiteFactory.Instance);
            SetProviderFactory("System.Data.SQLite.EF6", SQLiteProviderFactory.Instance);
            SetProviderServices("System.Data.SQLite", (DbProviderServices)SQLiteProviderFactory.Instance.GetService(typeof(DbProviderServices)));
        }
    }

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
