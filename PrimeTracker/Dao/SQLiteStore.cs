﻿using Innouvous.Utils.Data;
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
    class SQLiteStore : Innouvous.Utils.Data.SQLiteClient, IDataStore
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
                string sql = LoadFromText("SeriesTableTable", SeriesTable);
                ExecuteNonQuery(sql);

                sql = LoadFromText("CreateVideosTable", VideosTable, SeriesTable);
                ExecuteNonQuery(sql);

                sql = LoadFromText("CreateTagsTable", TagsTable, VideosTable);
                ExecuteNonQuery(sql);
            }
        }

        private Video ParseVideoRow(DataRow r)
        {
            var video = new Video()
            {
                AmazonId = r["AmazonId"].ToString(),
                Id = Convert.ToInt32(r["Id"]),
                Created = SQLUtils.ToDateTime(r["Created"].ToString()),
                Updated = SQLUtils.ToDateTime(r["Created"].ToString()),
                Type = (VideoType)Convert.ToInt32(r["Type"]),
                Url = r["Url"].ToString(),
                Description = Convert.ToString(r["Description"]),
                Title = r["Title"].ToString()
            };

            return video;
        }

        public Video GetVideoByAmazonId(string amazonId)
        {
            string sql = $"select * from {VideosTable} where Id = {amazonId}";

            var dt = ExecuteSelect(sql);

            if (dt.Rows.Count == 0)
                return null;
            else
                return ParseVideoRow(dt.Rows[0]);
        }

        public Video InsertVideo(Video v)
        {
            var txn = GetConnection().BeginTransaction();
            try
            {
                string expDate = v.ExpiringDate == null ? "NULL" : "'" + SQLUtils.ToSQLDateTime(v.ExpiringDate.Value) + "'";

                string cmd = $"INSERT INTO {VideosTable} VALUES(NULL, " +
                    $"'{SQLUtils.SQLEncode(v.Title)}', {(int)v.Type}, '{SQLUtils.SQLEncode(v.Url)}', " +
                    $"'{SQLUtils.SQLEncode(v.Description)}', '{SQLUtils.ToSQLDateTime(v.Created)}', " +
                    $"'{SQLUtils.ToSQLDateTime(v.Updated)}'," +
                    $" {expDate}, NULL)";

                ExecuteNonQuery(cmd);
                v.Id = SQLUtils.GetLastInsertRow(this);

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
                $"SET Updated = {SQLUtils.ToSQLDateTime(v.Updated)}" + //TODO: Add Others
                $" WHERE Id = {v.Id.Value}";

            ExecuteNonQuery(sql);
        }

        public List<Video> GetVideosByTag(TagTypes tag)
        {
            string cmd = $"SELECT v.* from {VideosTable} v " +
                $"JOIN {TagsTable} t ON v.Id = t.VideoId " +
                $"WHERE t.Value={(int)tag}";

            DataTable dt = ExecuteSelect(cmd);

            List<Video> videos = new List<Video>();

            foreach (DataRow r in dt.Rows)
            {
                videos.Add(ParseVideoRow(r));
            }

            return videos;
        }

        public void UpdateWatchlistIds(List<int> currentIds)
        {
            string sql = $"DELETE FROM {TagsTable} WHERE Value = {(int)TagTypes.WatchList} " +
                $"AND VideoId NOT IN ({string.Join(",", currentIds)})";

            ExecuteNonQuery(sql);
        }
    }
}