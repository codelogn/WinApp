using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Net.Http;
using System.Threading.Tasks;

namespace WindowsTaskbarApp.Jobs
{
    public class BackgroundJobs
    {
        // Remove the private isRunning field if you have a public property
        public bool IsRunning { get; private set; } = false;

        // Add a logs field or property
        public string Logs { get; private set; } = string.Empty;

        private readonly List<string> runningJobs = new List<string>();

        public event Action<string> JobStatusUpdated; // Event to notify job status updates

        public void Start()
        {
            if (IsRunning) return;

            IsRunning = true;
            Logs += $"[{DateTime.Now}] Job started.\n";
            Task.Run(async () =>
            {
                while (IsRunning)
                {
                    await ProcessAlertsAsync();
                    await Task.Delay(TimeSpan.FromSeconds(15)); // Wait for 15 seconds
                }
            });
        }

        public void Stop()
        {
            IsRunning = false;
            Logs += $"[{DateTime.Now}] Job stopped.\n";
        }

        private async Task ProcessAlertsAsync()
        {
            AddLog($"Processing jobs at {DateTime.Now}.. async process started");
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
                        int id = Convert.ToInt32(reader["Id"]);
                        string url = reader["URL"].ToString();
                        string keywords = reader["Keywords"].ToString();
                        string minutes = reader["Minutes"].ToString();

                        AddLog($"Processing job Id={id}, URL={url}, Keywords={keywords}, Minutes={minutes}");

                        //remove ! later
                        if (IsMinuteMatch(currentMinute, minutes))
                        {
                            string content = await FetchContentAsync(url);
                            AddLog($"Minutes matched! Job Id={id}, URL={url}, Content Length={content.Length}");
                            //AddLog($"Content fetched for job Id={id}, URL={url} content={content}");
                            AddLog($"Debug: content length={content?.Length ?? 0}, keywords='{keywords}'");
                            if (string.IsNullOrEmpty(content))
                            {
                                AddLog($"Warning: Content is null or empty for job Id={id}, URL={url}");
                            }
                            else if (string.IsNullOrEmpty(keywords))
                            {
                                AddLog($"Warning: Keywords are null or empty for job Id={id}, URL={url}");
                            }
                            else if (ContainsKeywords(content, keywords))
                            {
                                AddLog($"Keyword match found for job Id={id}, URL={url}");
                            }
                            else
                            {
                                AddLog($"No keyword match for job Id={id}, URL={url}");
                            }
                        }
                    }
                }
            }

            AddLog($"Finished processing jobs at {DateTime.Now}");
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
                AddLog($"Error fetching content from {url}: {ex.Message}");
                return string.Empty;
            }
        }

        private bool ContainsKeywords(string content, string keywords)
        {
            AddLog("Entering ContainsKeywords method.");
            if (string.IsNullOrEmpty(content))
            {
                AddLog("Content is null or empty in ContainsKeywords.");
                return false;
            }
            if (string.IsNullOrEmpty(keywords))
            {
                AddLog("Keywords are null or empty in ContainsKeywords.");
                return false;
            }

            var keywordList = keywords.Split(',');
            AddLog($"Split keywords: {string.Join("|", keywordList)}");

            foreach (var keyword in keywordList)
            {
                var trimmedKeyword = keyword.Trim();
                AddLog($"Checking keyword: '{trimmedKeyword}'");
                if (content.Contains(trimmedKeyword, StringComparison.OrdinalIgnoreCase))
                {
                    AddLog($"Keyword match found: '{trimmedKeyword}'");
                    return true;
                }
            }
            AddLog("No keyword match found in ContainsKeywords.");
            return false;
        }

        public List<string> GetRunningJobs()
        {
            return new List<string>(runningJobs);
        }

        // You can add a method to append logs from anywhere in the class
        public void AddLog(string message)
        {
            Logs += $"[{DateTime.Now}] {message}\n";
        }
        
        public void ClearLogs()
        {
            Logs = string.Empty;
            AddLog("Logs cleared.");
        }
    }
}