using System.Collections.Generic;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Linq;
using System.Diagnostics;

public static class GroupLinksService
{
    public static List<string> GetAllTags()
    {
            var tagsSet = new HashSet<string>();
            string connectionString = "Data Source=alerts.db;Version=3;";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Tags FROM Links WHERE Tags IS NOT NULL AND Tags != ''";
                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var cell = reader.GetString(0);
                        var splitTags = cell.Split(',');
                        foreach (var tag in splitTags)
                        {
                            var trimmed = tag.Trim();
                            if (!string.IsNullOrEmpty(trimmed))
                            {
                                tagsSet.Add(trimmed);
                            }
                        }
                    }
                }
            }
            return tagsSet.OrderBy(t => t).ToList();
    }


    public static void OpenLinksByTag(string tag)
    {
        string connectionString = "Data Source=alerts.db;Version=3;";
        var Links = new List<string>();

        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            string query = @"
                SELECT Link FROM Links
                WHERE ',' || Tags || ',' LIKE '%,' || @tag || ',%'
            ";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@tag", tag);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Links.Add(reader.GetString(0));
                    }
                }
            }
        }

        foreach (var Link in Links)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Link,
                    UseShellExecute = true
                });
            }
            catch
            {
                // Optionally handle errors
            }
        }
    }

    public static void BuildGroupLinksMenu(ToolStripMenuItem groupLinksMenu)
    {
        groupLinksMenu.DropDownItems.Clear();
        var tags = GetAllTags();
        foreach (var tag in tags)
        {
            var tagMenuItem = new ToolStripMenuItem(tag);
            tagMenuItem.Click += (s, e) => OpenLinksByTag(tag);
            groupLinksMenu.DropDownItems.Add(tagMenuItem);
        }
    }
}