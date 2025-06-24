using System;
using System.Configuration;
using System.Data.SQLite;

namespace WindowsTaskbarApp.Utils
{
    public static class LogUtil
    {
        public static void AddLog(string message, string level = "INFO", string source = null, string exception = null)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["AllInOneDb"].ConnectionString;
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"INSERT INTO Logs (Timestamp, Level, Message, Source, Exception) VALUES (@Timestamp, @Level, @Message, @Source, @Exception)";
                    command.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                    command.Parameters.AddWithValue("@Level", level ?? "INFO");
                    command.Parameters.AddWithValue("@Message", message);
                    command.Parameters.AddWithValue("@Source", source ?? "App");
                    command.Parameters.AddWithValue("@Exception", exception ?? string.Empty);
                    command.ExecuteNonQuery();
                }
                
            }
            catch (Exception ex)
            {
                // Optionally handle logging failure (e.g., write to file or ignore)
            }
        }
    }
}
