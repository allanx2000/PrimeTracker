using System.Collections.Generic;
using System.Text;
using PrimeTracker.Models;

namespace PrimeTracker
{
    internal class RefreshResults
    {
        private List<Video> added;
        private List<Video> failedIds;

        private string summary;

        public RefreshResults(List<Video> added, List<Video> failedIds)
        {
            this.added = added;
            this.failedIds = failedIds;
        }

        public string GetSummary()
        {
            if (summary == null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Added: ");
                added.Sort((a, b) => a.Title.CompareTo(b.Title));

                foreach (var v in added)
                {
                    sb.AppendLine("  " + v.Title);
                }

                sb.AppendLine("Failed: ");
                failedIds.Sort((a, b) => a.Title.CompareTo(b.Title));

                foreach (var v in failedIds)
                {
                    sb.AppendLine("  " + v.Title);
                }

                summary = sb.ToString();
            }

            return summary;
        }
    }
}