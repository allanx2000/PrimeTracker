using Innouvous.Utils.Data;
using PrimeTracker.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeTracker.Dao
{
    class SQLiteStore : SQLiteClient, IDataStore
    {
        private string dataFile;

        public SQLiteStore(string dataFile) : base(dataFile,
            !File.Exists(dataFile),
            new Dictionary<string, string>() {
                { "FKSupport", "True"}
            })
        {
            this.dataFile = dataFile;

            CreateTables();
        }

        private const string ScriptsPath = "TableScripts";
        private const string ScriptsFormat = "txt";

        private string LoadFromText(string name, params object[] args)
        {
            return SQLUtils.LoadCommandFromText(ScriptsPath, name, ScriptsFormat, args);
        }

        private const string SeriesTable = "tbl_series";
        private const string VideosTable = "tbl_videos";
        private const string TagsTable = "tbl_tags";

        private void CreateTables()
        {
            if (!SQLUtils.CheckTableExists(VideosTable, this))
            {
                string sql = LoadFromText("CreateSeriesTable", SeriesTable);
                ExecuteNonQuery(sql);

                sql = LoadFromText("CreateVideosTable", VideosTable, SeriesTable);
                ExecuteNonQuery(sql);

                sql = LoadFromText("CreateTagsTable", TagsTable, VideosTable);
                ExecuteNonQuery(sql);
            }
        }

        private Video ParseVideoRow(DataRow r, bool attachRelated)
        {
            var video = new Video()
            {
                AmazonId = r["AmazonId"].ToString(),
                Id = Convert.ToInt32(r["Id"]),
                Created = DateTime.Parse(r["Created"].ToString()),
                Updated = DateTime.Parse(r["Updated"].ToString()),
                Type = (VideoType)Convert.ToInt32(r["Type"]),
                Url = r["Url"].ToString(),
                Description = Convert.ToString(r["Description"]),
                Title = r["Title"].ToString()
            };

            if (attachRelated)
            {
                video.Tags = GetTags(video.Id.Value);
            }

            return video;
        }

        public Video GetVideoByAmazonId(string amazonId)
        {
            string sql = $"select * from {VideosTable} where AmazonId = '{amazonId}'";

            var dt = ExecuteSelect(sql);

            if (dt.Rows.Count == 0)
                return null;
            else
                return ParseVideoRow(dt.Rows[0], true);
        }

        public Video InsertVideo(Video v)
        {
            var txn = GetConnection().BeginTransaction();
            try
            {
                
                string cmd = $"INSERT INTO {VideosTable} VALUES(NULL, '{v.AmazonId}'," +
                    $"'{SQLUtils.SQLEncode(v.Title)}', {(int)v.Type}, '{SQLUtils.SQLEncode(v.Url)}', " +
                    $"'{SQLUtils.SQLEncode(v.Description)}', '{SQLUtils.ToSQLDateTime(v.Created)}', " +
                    $"'{SQLUtils.ToSQLDateTime(v.Updated)}'," +
                    $" NULL)"; //SeriesId

                ExecuteNonQuery(cmd);
                v.Id = SQLUtils.GetLastInsertRow(this);

                foreach (var t in v.Tags)
                    t.VideoId = v.Id.Value;

                UpdateAllTags(v);

                txn.Commit();

                return v;
            }
            catch (Exception e)
            {
                txn.Rollback();
                throw;
            }
        }

        private void UpdateAllTags(Video v)
        {
            string sql = $"DELETE FROM {TagsTable} WHERE VideoId = {v.Id}";
            ExecuteNonQuery(sql);

            if (v.Tags != null)
            {
                foreach (TagRecord tr in v.Tags)
                {
                    InsertTag(v.Id.Value, tr);
                }
            }

        }

        public void InsertTag(int videoId, TagRecord tr)
        {
            string cmd = $"INSERT INTO {TagsTable} VALUES({videoId}, {(int) tr.Value}, '{SQLUtils.ToSQLDateTime(tr.Added)}')";
            ExecuteNonQuery(cmd);
        }

        public void UpdateVideo(Video v)
        {
            string sql = $"UPDATE {VideosTable} " +
                $"SET Updated = '{SQLUtils.ToSQLDateTime(v.Updated)}'" + //TODO: Add Others
                $" WHERE Id = {v.Id.Value}";

            ExecuteNonQuery(sql);

            foreach (var t in v.Tags)
                t.VideoId = v.Id.Value;

            UpdateAllTags(v);
        }

        public List<Video> GetVideosByTag(TagTypes tag)
        {
            string cmd = $"SELECT VideoId from {TagsTable} WHERE Value={(int)tag}";

            DataTable dt = ExecuteSelect(cmd);

            List<Video> videos = new List<Video>();

            foreach (DataRow r in dt.Rows)
            {
                Video video = GetVideoById(Convert.ToInt32(r["VideoId"]));
                videos.Add(video);
            }

            return videos;
        }

        private Video GetVideoById(int id)
        {
            string cmd = $"select * from {VideosTable} where Id = {id}";

            DataTable dt = ExecuteSelect(cmd);
            if (dt.Rows.Count == 0)
                return null;

            Video v = ParseVideoRow(dt.Rows[0], true);
            return v;
        }

        private List<TagRecord> GetTags(int videoId)
        {
            string sql = $"select * from {TagsTable} where VideoId = {videoId}";
            DataTable dt = ExecuteSelect(sql);

            List<TagRecord> tags = new List<TagRecord>();

            foreach (DataRow r in dt.Rows)
            {
                TagRecord t = ParseTagRecord(r);
                tags.Add(t);
            }

            return tags;
        }

        private static TagRecord ParseTagRecord(DataRow r)
        {
            return new TagRecord()
            {
                Added = DateTime.Parse(r["Added"].ToString()),
                Value = (TagTypes)Convert.ToInt32(r["Value"]),
                VideoId = Convert.ToInt32(r["VideoId"])
            };
        }

        public void UpdateWatchlistIds(List<int> currentIds)
        {
            string sql = $"DELETE FROM {TagsTable} WHERE Value = {(int)TagTypes.WatchList} " +
                $"AND VideoId NOT IN ({string.Join(",", currentIds)})";

            ExecuteNonQuery(sql);
        }

        public List<Video> GetVideosByCreatedDate(int days)
        {
            var start = DateTime.Today.AddDays(-1 * days);
            string sql = $"select Id from {VideosTable} " +
                $"where Created >= '{SQLUtils.ToSQLDateTime(start) }'";

            var dt = ExecuteSelect(sql);

            List<Video> videos = new List<Video>();
            foreach (DataRow r in dt.Rows)
            {
                var v = GetVideoById(Convert.ToInt32(r["Id"]));
                if (!v.TagMap.ContainsKey(TagTypes.WatchList))
                {
                    videos.Add(v);
                }
            }

            return videos;
        }
    }
}
