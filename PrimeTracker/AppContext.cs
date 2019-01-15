using PrimeTracker.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
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

    class AppContext : DbContext
    {
        private static AppContext instance;

        public static AppContext InitializeAppContext(string dataFile)
        {
            instance = new AppContext(dataFile);
            return instance;
        }

        public static AppContext Instance
        {
            get { return instance; }
        }

        public DbSet<TagRecord> allTags { get; set; }
        public DbSet<TvSeries> tvSeries { get; set; }
        public DbSet<Video> allVideos { get; set; }

        private string dataFile;
        
        private AppContext(string dataFile) : base(CreateConnection(dataFile), true)
        {
            this.dataFile = dataFile;
        }

        private static SQLiteConnection CreateConnection(string dataFile)
        {
            return new SQLiteConnection()
            {
                ConnectionString = new SQLiteConnectionStringBuilder() {
                    DataSource = dataFile,
                    ForeignKeys = true }.ConnectionString
            };
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
        }
    }
}
