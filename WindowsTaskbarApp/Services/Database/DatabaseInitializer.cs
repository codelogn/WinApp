using System;
using System.Data.SQLite;
using System.IO;

namespace WindowsTaskbarApp.Services.Database
{
    public static class DatabaseInitializer
    {
        private const string DatabaseFileName = "alerts.db";
        private const string ConnectionString = "Data Source=alerts.db;Version=3;";

        public static void Initialize()
        {
            try
            {
                // Check if the database file exists
                if (!File.Exists(DatabaseFileName))
                {
                    // Create the database file
                    SQLiteConnection.CreateFile(DatabaseFileName);
                }

                // Ensure required tables exist
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    // Create the links table
                    CreateLinksTable(connection);

                    // Create the alerts table
                    CreateAlertsTable(connection);
                }
            }
            catch (Exception ex)
            {
                // Log or display the error
                throw new ApplicationException("Error initializing the database: " + ex.Message);
            }
        }

        private static void CreateLinksTable(SQLiteConnection connection)
        {
            var createLinksTableQuery = @"
                CREATE TABLE IF NOT EXISTS Links (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Link TEXT NOT NULL,
                    Tags TEXT DEFAULT '',
                    EnableAutoStartup INTEGER DEFAULT 0,
                    LastUpdated DATETIME DEFAULT CURRENT_TIMESTAMP
                );";

            using (var command = new SQLiteCommand(createLinksTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreateAlertsTable(SQLiteConnection connection)
        {
            var createAlertsTableQuery = @"
                CREATE TABLE IF NOT EXISTS Alerts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Topic TEXT NOT NULL,
                    LastUpdatedTime TEXT NOT NULL,
                    Minutes TEXT NOT NULL,
                    Keywords TEXT,
                    Query TEXT,
                    URL TEXT NOT NULL,
                    HTTPMethod TEXT NOT NULL,
                    HTTPBody TEXT,
                    Enabled TEXT NOT NULL,
                    ResponseType TEXT DEFAULT 'JSON',
                    ExecutionType TEXT DEFAULT 'Win Alert',
                    HTTPHeader TEXT,
                    LastTriggered TEXT
                );";

            using (var command = new SQLiteCommand(createAlertsTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}