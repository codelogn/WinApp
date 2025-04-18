using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Net.Http;
using System.Threading.Tasks;

namespace WindowsTaskbarApp.Jobs
{
    public class BackgroundJobs
    {
        private bool isRunning = false;
        private readonly List<string> runningJobs = new List<string>();

        public event Action<string> JobStatusUpdated; // Event to notify job status updates

        public void Start()
        {
            if (isRunning) return;

            isRunning = true;
            Task.Run(async () =>
            {
                while (isRunning)
                {
                    await ProcessAlertsAsync();
                    await Task.Delay(TimeSpan.FromMinutes(1)); // Wait for 1 minute
                }
            });
        }

        public void Stop()
        {
            isRunning = false;
        }

        private async Task ProcessAlertsAsync()
        {
            var now = DateTime.Now;
            int currentMinute = now.Minute;

            using (var connection = new SQLiteConnection("Data Source=alerts.db;Version=3;"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, URL, Keywords, Minutes FROM Alerts";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string url = reader["URL"].ToString();
                        string keywords = reader["Keywords"].ToString();
                        string minutes = reader["Minutes"].ToString();

                        if (IsMinuteMatch(currentMinute, minutes))
                        {
                            string content = await FetchContentAsync(url);

                            if (ContainsKeywords(content, keywords))
                            {
                                NotifyJobStatus($"Keyword match found for URL: {url}");
                            }
                        }
                    }
                }
            }
        }

        private bool IsMinuteMatch(int currentMinute, string minutes)
        {
            if (string.IsNullOrEmpty(minutes)) return false;

            var minuteList = minutes.Split(',');
            foreach (var minute in minuteList)
            {
                if (int.TryParse(minute.Trim(), out int parsedMinute) && parsedMinute == currentMinute)
                {
                    return true;
                }
            }
            return false;
        }

        private async Task<string> FetchContentAsync(string url)
        {
            try
            {
                using var httpClient = new HttpClient();
                return await httpClient.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                NotifyJobStatus($"Error fetching content from {url}: {ex.Message}");
                return string.Empty;
            }
        }

        private bool ContainsKeywords(string content, string keywords)
        {
            if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(keywords)) return false;

            var keywordList = keywords.Split(',');
            foreach (var keyword in keywordList)
            {
                if (content.Contains(keyword.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private void NotifyJobStatus(string message)
        {
            runningJobs.Add(message);
            JobStatusUpdated?.Invoke(message); // Notify listeners
        }

        public List<string> GetRunningJobs()
        {
            return new List<string>(runningJobs);
        }
    }
}