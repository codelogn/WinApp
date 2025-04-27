using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsTaskbarApp.Utils;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

namespace WindowsTaskbarApp.Forms.Alerts
{
    public partial class AlertsForm : Form
    {
        private SQLiteConnection connection;
        private DataGridView alertsGridView;
        private Button addButton;

        public AlertsForm()
        {
            InitializeComponent();
            this.Load += async (sender, e) =>
            {
                await InitializeDatabaseAsync();
                await LoadAlertsAsync();
            };
        }

        private void InitializeComponent()
        {
            this.alertsGridView = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
            this.addButton = new Button
            {
                Text = "Add Alert",
                Dock = DockStyle.Top,
                BackColor = Color.DarkBlue,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            this.addButton.Click += AddButton_Click;

            // Add data columns
            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "Id",
                DataPropertyName = "Id",
                Visible = false
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Topic",
                HeaderText = "Topic",
                DataPropertyName = "Topic"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "LastUpdatedTime",
                HeaderText = "Last Updated Time",
                DataPropertyName = "LastUpdatedTime"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Minutes",
                HeaderText = "Minutes",
                DataPropertyName = "Minutes"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Keywords",
                HeaderText = "Keywords",
                DataPropertyName = "Keywords"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Query",
                HeaderText = "Query",
                DataPropertyName = "Query"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "URL",
                HeaderText = "URL",
                DataPropertyName = "URL"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "HTTPMethod",
                HeaderText = "HTTP Method",
                DataPropertyName = "HTTPMethod" // Ensure this matches the database column name
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "HTTPHeader",
                HeaderText = "HTTP Header",
                DataPropertyName = "HTTPHeader"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "HTTPBody",
                HeaderText = "HTTP Body",
                DataPropertyName = "HTTPBody"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Enabled",
                HeaderText = "Enabled",
                DataPropertyName = "Enabled"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ResponseType",
                HeaderText = "Response Type",
                DataPropertyName = "ResponseType"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ExecutionType",
                HeaderText = "Execution Type",
                DataPropertyName = "ExecutionType"
            });

            // Add button columns
            var editButtonColumn = new DataGridViewButtonColumn
            {
                Name = "Edit",
                HeaderText = "Edit",
                Text = "Edit",
                UseColumnTextForButtonValue = true
            };
            alertsGridView.Columns.Add(editButtonColumn);

            var deleteButtonColumn = new DataGridViewButtonColumn
            {
                Name = "Delete",
                HeaderText = "Delete",
                Text = "Delete",
                UseColumnTextForButtonValue = true
            };
            alertsGridView.Columns.Add(deleteButtonColumn);

            var testButtonColumn = new DataGridViewButtonColumn
            {
                Name = "Test",
                HeaderText = "Test",
                Text = "Test",
                UseColumnTextForButtonValue = true
            };
            alertsGridView.Columns.Add(testButtonColumn);

            var openWebButtonColumn = new DataGridViewButtonColumn
            {
                Name = "OpenWeb",
                HeaderText = "Open Web",
                Text = "Open Web",
                UseColumnTextForButtonValue = true
            };
            alertsGridView.Columns.Add(openWebButtonColumn);

            alertsGridView.CellContentClick += AlertsGridView_CellContentClick;

            this.Controls.Add(this.alertsGridView);
            this.Controls.Add(this.addButton);

            this.Text = "Manage Alerts";
            this.Size = new System.Drawing.Size(800, 600);
        }

        private async Task InitializeDatabaseAsync()
        {
            var dbPath = "alerts.db";

            if (!System.IO.File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            await connection.OpenAsync();

            await InitializeDatabaseSchemaAsync(connection);
        }

        private static async Task InitializeDatabaseSchemaAsync(SQLiteConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Alerts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Topic TEXT NOT NULL,
                    LastUpdatedTime TEXT NOT NULL,
                    Minutes TEXT NOT NULL,
                    Keywords TEXT,
                    Query TEXT,
                    URL TEXT NOT NULL,
                    HTTPMethod TEXT NOT NULL, -- Renamed
                    HTTPBody TEXT,            -- Renamed
                    Enabled TEXT NOT NULL,
                    ResponseType TEXT DEFAULT 'JSON',
                    ExecutionType TEXT DEFAULT 'Win Alert',
                    HTTPHeader TEXT,          -- Renamed
                    LastTriggered TEXT
                );
            ";

            await Task.Run(() => command.ExecuteNonQuery());
        }

        private async Task LoadAlertsAsync()
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Alerts";

            var adapter = new SQLiteDataAdapter(command);
            var dataTable = new DataTable();

            await Task.Run(() => adapter.Fill(dataTable));

            alertsGridView.Invoke((Action)(() =>
            {
                alertsGridView.DataSource = dataTable;
            }));
        }

        private async void AddButton_Click(object sender, EventArgs e)
        {
            using (var detailsForm = new AlertDetailsForm(connection))
            {
                // Subscribe to the AlertSaved event
                detailsForm.AlertSaved += async (s, args) =>
                {
                    await LoadAlertsAsync(); // Refresh the grid view
                };

                detailsForm.ShowDialog();
            }
        }

        private async void AlertsGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var columnName = alertsGridView.Columns[e.ColumnIndex].Name;

            if (columnName == "Edit")
            {
                using (var detailsForm = new AlertDetailsForm(connection))
                {
                    // Populate fields
                    detailsForm.Id = int.TryParse(alertsGridView.Rows[e.RowIndex].Cells["Id"].Value?.ToString(), out var parsedId) ? parsedId : (int?)null;
                    detailsForm.Topic = alertsGridView.Rows[e.RowIndex].Cells["Topic"].Value?.ToString();
                    detailsForm.LastUpdatedTime = alertsGridView.Rows[e.RowIndex].Cells["LastUpdatedTime"].Value?.ToString();
                    detailsForm.Minutes = alertsGridView.Rows[e.RowIndex].Cells["Minutes"].Value?.ToString();
                    detailsForm.Keywords = alertsGridView.Rows[e.RowIndex].Cells["Keywords"].Value?.ToString();
                    detailsForm.Query = alertsGridView.Rows[e.RowIndex].Cells["Query"].Value?.ToString();
                    detailsForm.URL = alertsGridView.Rows[e.RowIndex].Cells["URL"].Value?.ToString();
                    detailsForm.HTTPMethod = alertsGridView.Rows[e.RowIndex].Cells["HTTPMethod"].Value?.ToString();
                    detailsForm.HTTPBody = alertsGridView.Rows[e.RowIndex].Cells["HTTPBody"].Value?.ToString();
                    detailsForm.Enabled = alertsGridView.Rows[e.RowIndex].Cells["Enabled"].Value?.ToString();
                    detailsForm.ResponseType = alertsGridView.Rows[e.RowIndex].Cells["ResponseType"].Value?.ToString();
                    detailsForm.ExecutionType = alertsGridView.Rows[e.RowIndex].Cells["ExecutionType"].Value?.ToString();
                    detailsForm.HTTPHeader = alertsGridView.Rows[e.RowIndex].Cells["HTTPHeader"].Value?.ToString();

                    // Subscribe to the AlertSaved event
                    detailsForm.AlertSaved += async (s, args) =>
                    {
                        await LoadAlertsAsync(); // Refresh the grid view
                    };

                    detailsForm.ShowDialog();
                }
            }
            else if (columnName == "Delete")
            {
                // Delete the selected alert
                var id = alertsGridView.Rows[e.RowIndex].Cells["Id"].Value?.ToString();
                if (MessageBox.Show("Are you sure you want to delete this alert?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "DELETE FROM Alerts WHERE Id = @id";
                            command.Parameters.AddWithValue("@id", id);

                            await Task.Run(() => command.ExecuteNonQuery());
                        }

                        await LoadAlertsAsync(); // Automatically refresh the grid view
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while deleting the alert: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else if (columnName == "Test")
            {
                // Test the URL in an embedded browser
                var url = alertsGridView.Rows[e.RowIndex].Cells["URL"].Value?.ToString();
                Browser.OpenInEmbeddedBrowser(url);
            }
            else if (columnName == "OpenWeb")
            {
                var url = alertsGridView.Rows[e.RowIndex].Cells["URL"].Value?.ToString();
                var minutesValue = alertsGridView.Rows[e.RowIndex].Cells["Minutes"].Value?.ToString();

                if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(minutesValue) || !int.TryParse(minutesValue, out var minutes))
                {
                    MessageBox.Show("Invalid URL or Minutes value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var browserForm = new FullScreenBrowserForm(url, minutes);
                browserForm.Show();
            }
        }
    }


    // Add a new form for the full-screen browser
    public class FullScreenBrowserForm : Form
    {
        private readonly string url;
        private readonly int refreshIntervalMinutes;
        private readonly WebView2 webView;
        private readonly Timer refreshTimer;
        private readonly Timer mouseCheckTimer; // Timer to check mouse position
        private readonly Label countdownLabel;
        private readonly Button closeButton;
        private readonly Button windowedButton;
        private readonly Button fullScreenButton;
        private readonly Button refreshButton; // New Refresh Button
        private readonly Label loadingLabel; // Loading label
        private int remainingSeconds;

        public FullScreenBrowserForm(string url, int refreshIntervalMinutes)
        {
            this.url = url;
            this.refreshIntervalMinutes = refreshIntervalMinutes;
            this.remainingSeconds = refreshIntervalMinutes * 60;

            // Initialize WebView2
            this.webView = new WebView2 { Dock = DockStyle.Fill };
            this.webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
            this.webView.NavigationStarting += WebView_NavigationStarting;
            this.webView.NavigationCompleted += WebView_NavigationCompleted;
            this.Controls.Add(webView);
            webView.SendToBack(); // Ensure WebView2 is rendered behind other controls

            // Initialize Countdown Label
            this.countdownLabel = new Label
            {
                Text = FormatTime(remainingSeconds),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(100, 0, 0, 0), // More transparent black
                Font = new Font("Arial", 8, FontStyle.Bold), // Reduced font size by 50%
                AutoSize = true,
                Padding = new Padding(8), // Reduced padding for smaller size
                Visible = true
            };
            this.Controls.Add(countdownLabel);
            countdownLabel.BringToFront();

            // Initialize Close Button
            this.closeButton = new Button
            {
                Text = "X",
                ForeColor = Color.White,
                BackColor = Color.Red,
                Font = new Font("Arial", 14, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(50, 50),
                Visible = false // Initially hidden
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.FlatAppearance.MouseOverBackColor = Color.DarkRed; // Hover effect
            closeButton.Click += (sender, e) => this.Close();
            this.Controls.Add(closeButton);

            // Initialize Windowed Button
            this.windowedButton = new Button
            {
                Text = "☐", // Unicode for a window icon
                ForeColor = Color.White,
                BackColor = Color.Gray,
                Font = new Font("Arial", 14, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(50, 50),
                Visible = false // Initially hidden
            };
            windowedButton.FlatAppearance.BorderSize = 0;
            windowedButton.FlatAppearance.MouseOverBackColor = Color.DarkGray; // Hover effect
            windowedButton.Click += (sender, e) => ExitFullScreen();
            this.Controls.Add(windowedButton);

            // Initialize Full Screen Button
            this.fullScreenButton = new Button
            {
                Text = "⛶", // Unicode for a full-screen icon
                ForeColor = Color.White,
                BackColor = Color.Gray,
                Font = new Font("Arial", 14, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(50, 50),
                Visible = false // Initially hidden
            };
            fullScreenButton.FlatAppearance.BorderSize = 0;
            fullScreenButton.FlatAppearance.MouseOverBackColor = Color.DarkGray; // Hover effect
            fullScreenButton.Click += (sender, e) => EnterFullScreen();
            this.Controls.Add(fullScreenButton);

            // Initialize Refresh Button
            this.refreshButton = new Button
            {
                Text = "⟳", // Unicode for a refresh icon
                ForeColor = Color.White,
                BackColor = Color.Green,
                Font = new Font("Arial", 14, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(50, 50),
                Visible = false // Initially hidden
            };
            refreshButton.FlatAppearance.BorderSize = 0;
            refreshButton.FlatAppearance.MouseOverBackColor = Color.DarkGreen; // Hover effect
            refreshButton.Click += (sender, e) =>
            {
                if (webView.CoreWebView2 != null)
                {
                    webView.Reload(); // Reload the current page
                }
                else
                {
                    LoadUrl(); // Load the URL if WebView2 is not initialized
                }
            };
            this.Controls.Add(refreshButton);

            // Initialize Loading Label
            this.loadingLabel = new Label
            {
                Text = "Loading...",
                ForeColor = Color.White,
                BackColor = Color.FromArgb(128, 0, 0, 0), // Semi-transparent black
                Font = new Font("Arial", 14, FontStyle.Bold),
                AutoSize = true,
                Padding = new Padding(10),
                Visible = false // Initially hidden
            };
            this.Controls.Add(loadingLabel);
            loadingLabel.BringToFront();

            // Position the label and button
            PositionOverlayElements();

            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;

            // Initialize Timers
            this.refreshTimer = new Timer { Interval = 1000 }; // Tick every second
            this.refreshTimer.Tick += RefreshTimer_Tick;

            this.mouseCheckTimer = new Timer { Interval = 100 }; // Check mouse position every 100ms
            this.mouseCheckTimer.Tick += MouseCheckTimer_Tick;
            this.mouseCheckTimer.Start();

            this.Load += async (sender, e) =>
            {
                await InitializeWebViewAsync();
                LoadUrl();
                refreshTimer.Start();
                PositionOverlayElements();
            };

            this.FormClosing += (sender, e) =>
            {
                refreshTimer.Stop();
                refreshTimer.Dispose();
                mouseCheckTimer.Stop();
                mouseCheckTimer.Dispose(); // Stop and dispose of the mouseCheckTimer
            };

            this.Resize += (sender, e) => PositionOverlayElements();
        }

        private void MouseCheckTimer_Tick(object sender, EventArgs e)
        {
            if (this.IsDisposed) return; // Prevent accessing a disposed object
            var mousePosition = this.PointToClient(Cursor.Position);
            bool isMouseNearTop = mousePosition.Y <= 50; // Adjusted to align better with buttons

            // Show or hide buttons based on mouse position
            closeButton.Visible = isMouseNearTop;
            windowedButton.Visible = isMouseNearTop;
            fullScreenButton.Visible = isMouseNearTop;
            refreshButton.Visible = isMouseNearTop;

            if (isMouseNearTop)
            {
                closeButton.BringToFront();
                windowedButton.BringToFront();
                fullScreenButton.BringToFront();
                refreshButton.BringToFront();
            }
        }

        private async Task InitializeWebViewAsync()
        {
            await webView.EnsureCoreWebView2Async(null);
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            remainingSeconds--;

            if (remainingSeconds <= 0)
            {
                LoadUrl();
                remainingSeconds = refreshIntervalMinutes * 60; // Reset countdown
            }

            countdownLabel.Text = FormatTime(remainingSeconds);
        }

        private void LoadUrl()
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("The URL is invalid or empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string formattedUrl = url;
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                formattedUrl = "http://" + url; // Default to HTTP if no scheme is provided
            }

            try
            {
                if (webView.CoreWebView2 != null && webView.Source != null && webView.Source.ToString() == formattedUrl)
                {
                    // If the URL is already loaded, reload the page
                    webView.Reload();
                }
                else
                {
                    // Otherwise, navigate to the new URL
                    webView.Source = new Uri(formattedUrl);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to navigate to the URL: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string FormatTime(int totalSeconds)
        {
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"Refresh in: {minutes:D2}:{seconds:D2}";
        }

        private void PositionOverlayElements()
        {
            // Adjust the size and position of the countdown label
            countdownLabel.Location = new Point(20, 20); // Top-left corner with padding
            countdownLabel.BackColor = Color.FromArgb(100, 0, 0, 0); // More transparent black
            countdownLabel.Padding = new Padding(8); // Reduced padding for smaller size

            // Adjust the size and position of the close button
            closeButton.Size = new Size(50, 50);
            closeButton.Location = new Point(this.ClientSize.Width - closeButton.Width - 20, 20); // Add padding from the right and top

            // Adjust the size and position of the windowed button
            windowedButton.Size = new Size(50, 50);
            windowedButton.Location = new Point(closeButton.Location.X - windowedButton.Width - 10, 20); // Add spacing between buttons

            // Adjust the size and position of the full-screen button
            fullScreenButton.Size = new Size(50, 50);
            fullScreenButton.Location = new Point(windowedButton.Location.X - fullScreenButton.Width - 10, 20); // Add spacing between buttons

            // Adjust the size and position of the refresh button
            refreshButton.Size = new Size(50, 50);
            refreshButton.Location = new Point(fullScreenButton.Location.X - refreshButton.Width - 10, 20); // Add spacing between buttons

            // Center the loading label
            loadingLabel.Location = new Point(
                (this.ClientSize.Width - loadingLabel.Width) / 2,
                (this.ClientSize.Height - loadingLabel.Height) / 2
            );
        }

        private void EnterFullScreen()
        {
            this.WindowState = FormWindowState.Normal; // Reset to normal first to avoid issues
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true; // Ensure the form is on top of all other windows
            fullScreenButton.Visible = false;
            windowedButton.Visible = true;
        }

        private void ExitFullScreen()
        {
            this.WindowState = FormWindowState.Normal;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.TopMost = false; // Allow other windows to appear on top
            fullScreenButton.Visible = true;
            windowedButton.Visible = false;
        }

        private void WebView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            // Show the loading label when navigation starts
            loadingLabel.Visible = true;
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // Hide the loading label when navigation is complete
            loadingLabel.Visible = false;
        }

        private void WebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                MessageBox.Show("Failed to initialize WebView2.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}