using System;
using System.Configuration;
using System.Data.SQLite;
using System.IO;

namespace WindowsTaskbarApp.Services.Database
{
    public static class DatabaseInitializer
    {
        private static string ConnectionString => ConfigurationManager.ConnectionStrings["AllInOneDb"].ConnectionString;
        private static string DatabaseFileName => new SQLiteConnectionStringBuilder(ConnectionString).DataSource;

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

                    // Create the logs table
                    CreateLogsTable(connection);

                    // Create the configurations table
                    CreateConfigurationsTable(connection);
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
                    HTTPHeader TEXT,
                    ContentType TEXT,
                    Accept TEXT,
                    UserAgent TEXT,
                    Enabled TEXT NOT NULL,
                    ResponseType TEXT DEFAULT 'JSON',
                    ExecutionType TEXT DEFAULT 'Win Alert',
                    LastTriggered TEXT
                );";

            using (var command = new SQLiteCommand(createAlertsTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreateLogsTable(SQLiteConnection connection)
        {
            var createLogsTableQuery = @"
                CREATE TABLE IF NOT EXISTS Logs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Timestamp DATETIME NOT NULL,
                    Level TEXT NOT NULL,
                    Message TEXT NOT NULL,
                    Source TEXT,
                    Exception TEXT
                );";

            using (var command = new SQLiteCommand(createLogsTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreateConfigurationsTable(SQLiteConnection connection)
        {
            var createConfigurationsTableQuery = @"
                CREATE TABLE IF NOT EXISTS Configurations (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT,
                    Key TEXT NOT NULL,
                    Value TEXT
                );";

            using (var command = new SQLiteCommand(createConfigurationsTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }

    }
}