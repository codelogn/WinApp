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
            string lastUpdatedTime, // Renamed parameter
            string minutes,
            string keywords,
            string url,
            string method,
            string body,
            string enabled,
            string lastTriggered,
            string responseType, // Add ResponseType parameter
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
                        URL = @url, Method = @method, Body = @body, Enabled = @enabled, LastTriggered = @lastTriggered,
                        ResponseType = @responseType
                    WHERE Id = @id";
                command.Parameters.AddWithValue("@id", id.Value);
            }
            else // Insert new alert
            {
                command.CommandText = @"
                    INSERT INTO Alerts (Topic, LastUpdatedTime, Minutes, Keywords, URL, Method, Body, Enabled, LastTriggered, ResponseType)
                    VALUES (@topic, @lastUpdatedTime, @minutes, @keywords, @url, @method, @body, @enabled, @lastTriggered, @responseType)";
            }

            // Add parameters
            command.Parameters.AddWithValue("@topic", topic);
            command.Parameters.AddWithValue("@lastUpdatedTime", lastUpdatedTime); // Updated parameter
            command.Parameters.AddWithValue("@minutes", minutes);
            command.Parameters.AddWithValue("@keywords", keywords);
            command.Parameters.AddWithValue("@url", url);
            command.Parameters.AddWithValue("@method", method);
            command.Parameters.AddWithValue("@body", body);
            command.Parameters.AddWithValue("@enabled", enabled);
            command.Parameters.AddWithValue("@lastTriggered", lastTriggered);
            command.Parameters.AddWithValue("@responseType", responseType ?? "JSON"); // Default to JSON if null

            // Execute the command
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