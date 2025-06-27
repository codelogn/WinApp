using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Net.Http;
using System.Threading.Tasks;
using WindowsTaskbarApp.Services.HttpClient;
using WindowsTaskbarApp.Utils;

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

            string connectionString = ConfigurationManager.ConnectionStrings["AllInOneDb"].ConnectionString;
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, URL, Keywords, CheckIntervalMinutes, LastTriggered, HTTPMethod, HTTPHeader, HTTPBody, ContentType, Accept, UserAgent FROM Alerts";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["Id"]);
                        string url = reader["URL"].ToString();
                        string keywords = reader["Keywords"].ToString();
                        string checkIntervalMinutesStr = reader["CheckIntervalMinutes"].ToString();
                        string lastTriggeredStr = reader["LastTriggered"].ToString();
                        string httpMethod = reader["HTTPMethod"].ToString();
                        string httpHeader = reader["HTTPHeader"].ToString();
                        string httpBody = reader["HTTPBody"].ToString();
                        string contentType = reader["ContentType"].ToString();
                        string accept = reader["Accept"].ToString();
                        string userAgent = reader["UserAgent"].ToString();

                        AddLog($"Processing job Id={id}, URL={url}, Keywords={keywords}, CheckIntervalMinutes={checkIntervalMinutesStr}, LastTriggered={lastTriggeredStr}");

                        if (!int.TryParse(checkIntervalMinutesStr, out int checkIntervalMinutes) || checkIntervalMinutes <= 0)
                        {
                            AddLog($"Invalid CheckIntervalMinutes for job Id={id}. Skipping.");
                            continue;
                        }

                        DateTime lastTriggered = DateTime.MinValue;
                        if (!string.IsNullOrWhiteSpace(lastTriggeredStr))
                            DateTime.TryParse(lastTriggeredStr, out lastTriggered);
                        var nowTime = DateTime.Now;
                        var minutesSinceLast = (nowTime - lastTriggered).TotalMinutes;
                        if (lastTriggered == DateTime.MinValue || minutesSinceLast >= checkIntervalMinutes)
                        {
                            string content = await HttpHelper.FetchContentAsync(url, httpMethod, httpHeader, httpBody, contentType, accept, userAgent);
                            AddLog($"Interval matched! Job Id={id}, URL={url}, Content Length={content.Length}");
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
                                // Insert event into Events table
                                try
                                {
                                    using (var insertConn = new SQLiteConnection(connectionString))
                                    {
                                        insertConn.Open();
                                        var insertCmd = insertConn.CreateCommand();
                                        insertCmd.CommandText = @"INSERT INTO Events (EventName, EventContent, CreateDate, AlertId, SourceUrl, NotificationType) VALUES (@eventName, @eventContent, @createDate, @alertId, @sourceUrl, @notificationType)";
                                        insertCmd.Parameters.AddWithValue("@eventName", $"Keyword Match for Alert {id}");
                                        insertCmd.Parameters.AddWithValue("@eventContent", content.Length > 1000 ? content.Substring(0, 1000) : content); // Limit content size
                                        insertCmd.Parameters.AddWithValue("@createDate", DateTime.Now);
                                        insertCmd.Parameters.AddWithValue("@alertId", id);
                                        insertCmd.Parameters.AddWithValue("@sourceUrl", url);
                                        insertCmd.Parameters.AddWithValue("@notificationType", "KeywordMatch");
                                        insertCmd.ExecuteNonQuery();
                                    }
                                    AddLog($"Event inserted into Events table for AlertId={id}");
                                }
                                catch (Exception ex)
                                {
                                    AddLog($"Error inserting event for AlertId={id}: {ex.Message}", "ERROR");
                                }
                                // Update LastTriggered to now
                                try
                                {
                                    using (var updateConn = new SQLiteConnection(connectionString))
                                    {
                                        updateConn.Open();
                                        var updateCmd = updateConn.CreateCommand();
                                        updateCmd.CommandText = "UPDATE Alerts SET LastTriggered = @now WHERE Id = @id";
                                        updateCmd.Parameters.AddWithValue("@now", nowTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                        updateCmd.Parameters.AddWithValue("@id", id);
                                        updateCmd.ExecuteNonQuery();
                                    }
                                    AddLog($"Updated LastTriggered for AlertId={id}");
                                }
                                catch (Exception ex)
                                {
                                    AddLog($"Error updating LastTriggered for AlertId={id}: {ex.Message}", "ERROR");
                                }
                            }
                            else
                            {
                                AddLog($"No keyword match for job Id={id}, URL={url}");
                            }
                        }
                        else
                        {
                            AddLog($"Interval not reached for job Id={id}. Minutes since last: {minutesSinceLast:F2}");
                        }
                    }
                }
            }

            AddLog($"Finished processing jobs at {DateTime.Now}");
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
        public void AddLog(string message, string level = "INFO", string source = null, string exception = null)
        {
            Logs += $"[{DateTime.Now}] {message}\n";
            LogUtil.AddLog(message, level, source ?? nameof(BackgroundJobs), exception);
        }
        
        public void ClearLogs()
        {
            Logs = string.Empty;
            AddLog("Logs cleared.");
        }
    }
}