using System;
using System.Configuration;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace WindowsTaskbarApp.Tools.Links
{
    public class LinksTrayIcon : IDisposable
    {
        private NotifyIcon trayIcon;
        private SQLiteConnection connection;
        private ContextMenuStrip trayMenu;

        public void Initialize()
        {
            // Ensure only one tray icon is created
            if (trayIcon != null)
                return;

            // Create the tray icon
            trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application, // Use a default system icon
                Text = "Links",
                Visible = true
            };

            // Create the context menu for the tray icon
            trayMenu = new ContextMenuStrip();
            trayIcon.ContextMenuStrip = trayMenu;

            // Load menu items dynamically
            LoadTrayMenu();

            // Add an Exit option
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Exit", null, (s, e) => Application.Exit());

            // Handle left-click to refresh the menu and show it
            trayIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    // Refresh the menu
                    LoadTrayMenu();

                    // Show the context menu at the cursor position
                    trayMenu.Show(Cursor.Position);
                }
            };

            // Hide menu when clicking outside
            trayMenu.Closed += (s, e) =>
            {
                if (e.CloseReason == ToolStripDropDownCloseReason.AppClicked)
                {
                    trayMenu.Hide();
                }
            };
        }

        private void LoadTrayMenu()
        {
            try
            {
                // Clear existing menu items
                trayMenu.Items.Clear();

                // Connect to the SQLite database
                string connectionString = ConfigurationManager.ConnectionStrings["AllInOneDb"].ConnectionString;
                connection = new SQLiteConnection(connectionString);
                connection.Open();

                var query = "SELECT Title, Link FROM links";
                var command = new SQLiteCommand(query, connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var title = reader["Title"].ToString();
                    var link = reader["Link"].ToString();

                    // Add menu item for each link
                    var menuItem = new ToolStripMenuItem(title);
                    menuItem.Click += (s, e) => OpenLink(link);
                    trayMenu.Items.Add(menuItem);
                }

                // Add a separator and the Exit option
                //trayMenu.Items.Add(new ToolStripSeparator());
                //trayMenu.Items.Add("Exit", null, (s, e) => ExitTrayApp());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tray menu: {ex.Message}", "Error");
            }
            finally
            {
                connection?.Close();
            }
        }

        private void OpenLink(string link)
        {
            try
            {
                if (link.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    // Open in default browser
                    Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
                }
                else if (File.Exists(link) || Directory.Exists(link))
                {
                    // Open file or folder
                    Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show("The specified link is invalid or does not exist.", "Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening link: {ex.Message}", "Error");
            }
        }

        private void ExitTrayApp()
        {
            try
            {
                // Dispose of the tray icon
                Dispose();

                // Exit the message loop for the tray app
                Application.ExitThread();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exiting tray app: {ex.Message}", "Error");
            }
        }

        public void Dispose()
        {
            // Dispose of the tray icon when the application exits
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
                trayIcon = null;
            }
        }
    }
}