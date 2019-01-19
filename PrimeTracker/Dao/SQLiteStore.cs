using Innouvous.Utils.Data;
using System;
using System.Collections.Generic;
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


        public void UpdateIssue(Issue issue)
        {
            string sql = $"update {IssuesTable} set is_new = {ToBitFlag(issue.IsNew)}, " +
                $"is_deleted = { ToBitFlag(issue.Deleted)} where issue_id = {issue.ID}";

            client.ExecuteNonQuery(sql);
        }

        private string ToBitFlag(bool val)
        {
            return val ? "1" : "NULL";
        }

        public bool InsertIssue(Issue x)
        {
            if (!HasIssue(x.ID))
            {
                string sql = $"insert into {IssuesTable} values({x.ID},'{x.Title}','{x.IssueUrl}'," +
                    $"'{x.ImageUrl}', NULL, 1)";

                client.ExecuteNonQuery(sql);
                return true;
            }
            return false;
        }

        public bool HasIssue(int id)
        {
            int ct = Convert.ToInt32(client.ExecuteScalar($"select count(*) from {IssuesTable} where issue_id={id}"));
            return ct > 0;
        }
        public List<Issue> GetIssues(SearchFilter filter = SearchFilter.New, int? limit = null)
        {
            string where;

            switch (filter)
            {
                case SearchFilter.New:
                    where = "is_new = 1";
                    break;
                case SearchFilter.Deleted:
                    where = "is_deleted = 1";
                    break;
                case SearchFilter.Read:
                    where = "is_new is NULL and is_deleted is NULL";
                    break;
                default:
                    throw new Exception("SearchFilter not recognized.");
            }

            string sql = $"select * from {IssuesTable} where {where} order by issue_id desc";

            if (limit != null)
            {
                sql += " limit " + limit.Value;
            }

            DataTable dt = client.ExecuteSelect(sql);

            List<Issue> issues = new List<Issue>();
            foreach (DataRow r in dt.Rows)
            {
                issues.Add(ParseIssueRow(r));
            }

            return issues;
        }

        private Issue ParseIssueRow(DataRow r)
        {
            var issue = new Issue()
            {
                Deleted = !SQLUtils.IsNull(r["is_deleted"]),
                ID = Convert.ToInt32(r["issue_id"]),
                ImageUrl = r["image_url"].ToString(),
                IsNew = !SQLUtils.IsNull(r["is_new"]),
                IssueUrl = r["issue_url"].ToString(),
                Title = r["title"].ToString()
            };

            return issue;
        }

        public Issue GetIssue(int issueId)
        {
            string sql = $"select * from {IssuesTable} where issue_id = {issueId}";

            var dt = client.ExecuteSelect(sql);

            if (dt.Rows.Count == 0)
                return null;
            else
                return ParseIssueRow(dt.Rows[0]);
        }
    }
}
