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
            string httpMethod,   // Renamed to HTTPMethod
            string httpBody,     // Renamed to HTTPBody
            string enabled,
            string lastTriggered,
            string responseType,
            string httpHeader,   // Renamed to HTTPHeader
            string executionType, // Added ExecutionType
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
                        ExecutionType = @executionType
                    WHERE Id = @id";
                command.Parameters.AddWithValue("@id", id.Value);
            }
            else // Insert new alert
            {
                command.CommandText = @"
                    INSERT INTO Alerts (Topic, LastUpdatedTime, Minutes, Keywords, Query, URL, HTTPMethod, HTTPBody, Enabled, 
                                        LastTriggered, ResponseType, HTTPHeader, ExecutionType)
                    VALUES (@topic, @lastUpdatedTime, @minutes, @keywords, @query, @url, @httpMethod, @httpBody, @enabled, 
                            @lastTriggered, @responseType, @httpHeader, @executionType)";
            }

            // Add parameters
            command.Parameters.AddWithValue("@topic", topic);
            command.Parameters.AddWithValue("@lastUpdatedTime", DateTime.Now);
            command.Parameters.AddWithValue("@minutes", minutes);
            command.Parameters.AddWithValue("@keywords", keywords);
            command.Parameters.AddWithValue("@query", query);
            command.Parameters.AddWithValue("@url", url);
            command.Parameters.AddWithValue("@httpMethod", httpMethod); // Updated
            command.Parameters.AddWithValue("@httpBody", httpBody);     // Updated
            command.Parameters.AddWithValue("@enabled", enabled);
            command.Parameters.AddWithValue("@lastTriggered", lastTriggered);
            command.Parameters.AddWithValue("@responseType", responseType ?? "JSON");
            command.Parameters.AddWithValue("@httpHeader", httpHeader); // Updated
            command.Parameters.AddWithValue("@executionType", executionType); // Added ExecutionType

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