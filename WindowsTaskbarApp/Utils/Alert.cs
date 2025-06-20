using System;
using System.Data.SQLite;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsTaskbarApp.Utils
{
    public static class Alert
    {
        public static async Task SaveAlertAsync(
            SQLiteConnection connection,
            string topic,
            string lastUpdatedTime,
            string minutes,
            string keywords,
            string query,
            string url,
            string httpMethod,
            string httpBody,
            string enabled,
            string lastTriggered,
            string responseType,
            string httpHeader,
            string executionType,
            string contentType,    // Added
            string accept,         // Added
            string userAgent,      // Added
            int? id = null)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            var command = connection.CreateCommand();

            if (id.HasValue) // Update existing alert
            {
                command.CommandText = @"
                    UPDATE Alerts
                    SET Topic = @topic, LastUpdatedTime = @lastUpdatedTime, Minutes = @minutes, Keywords = @keywords,
                        Query = @query, URL = @url, HTTPMethod = @httpMethod, HTTPBody = @httpBody, Enabled = @enabled, 
                        LastTriggered = @lastTriggered, ResponseType = @responseType, HTTPHeader = @httpHeader,
                        ExecutionType = @executionType,
                        ContentType = @contentType, Accept = @accept, UserAgent = @userAgent
                    WHERE Id = @id";
                command.Parameters.AddWithValue("@id", id.Value);
            }
            else // Insert new alert
            {
                command.CommandText = @"
                    INSERT INTO Alerts (Topic, LastUpdatedTime, Minutes, Keywords, Query, URL, HTTPMethod, HTTPBody, Enabled, 
                                        LastTriggered, ResponseType, HTTPHeader, ExecutionType, ContentType, Accept, UserAgent)
                    VALUES (@topic, @lastUpdatedTime, @minutes, @keywords, @query, @url, @httpMethod, @httpBody, @enabled, 
                            @lastTriggered, @responseType, @httpHeader, @executionType, @contentType, @accept, @userAgent)";
            }

            // Add parameters
            command.Parameters.AddWithValue("@topic", topic);
            command.Parameters.AddWithValue("@lastUpdatedTime", DateTime.Now);
            command.Parameters.AddWithValue("@minutes", minutes);
            command.Parameters.AddWithValue("@keywords", keywords);
            command.Parameters.AddWithValue("@query", query);
            command.Parameters.AddWithValue("@url", url);
            command.Parameters.AddWithValue("@httpMethod", httpMethod);
            command.Parameters.AddWithValue("@httpBody", httpBody);
            command.Parameters.AddWithValue("@enabled", enabled);
            command.Parameters.AddWithValue("@lastTriggered", lastTriggered);
            command.Parameters.AddWithValue("@responseType", responseType ?? "JSON");
            command.Parameters.AddWithValue("@httpHeader", httpHeader);
            command.Parameters.AddWithValue("@executionType", executionType);
            command.Parameters.AddWithValue("@contentType", contentType ?? string.Empty); // Added
            command.Parameters.AddWithValue("@accept", accept ?? string.Empty);           // Added
            command.Parameters.AddWithValue("@userAgent", userAgent ?? string.Empty);     // Added

            await Task.Run(() => command.ExecuteNonQuery());
        }

        public static async Task TestUrlAsync(string url)
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    var response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode(); // Throws if the status code is not 2xx
                    MessageBox.Show("URL tested successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error testing URL: {ex.Message}");
                }
            }
        }
    }
}