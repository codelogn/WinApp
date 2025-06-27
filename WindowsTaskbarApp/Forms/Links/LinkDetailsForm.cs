using System;
using System.Data.SQLite;
using System.Linq;
using System.Windows.Forms;
using WindowsTaskbarApp.Tools.GroupLinks;
//using IWshRuntimeLibrary; // Add this at the top
using System.IO;

namespace WindowsTaskbarApp.Forms.Links
{
    public partial class LinkDetailsForm : Form
    {
        private readonly SQLiteConnection connection;
        private readonly int? linkId; // Nullable to differentiate between Add and Edit
        private TextBox titleTextBox;
        private TextBox linkTextBox;
        private TextBox tagsTextBox;
        private CheckBox enableAutoStartupCheckBox;
        private Button saveButton;

        public LinkDetailsForm(SQLiteConnection connection, int? linkId = null, string title = "", string link = "", string tags = "", bool enableAutoStartup = false)
        {
            if (connection == null || connection.State != System.Data.ConnectionState.Open)
            {
                throw new ArgumentException("Database connection must be initialized and open.", nameof(connection));
            }

            this.connection = connection;
            this.linkId = linkId;

            InitializeComponent();

            titleTextBox.Text = title;
            linkTextBox.Text = link;
            tagsTextBox.Text = tags;
            enableAutoStartupCheckBox.Checked = enableAutoStartup;
        }

        private void InitializeComponent()
        {
            this.Text = linkId.HasValue ? "Edit Link" : "Add Link";
            this.Size = new System.Drawing.Size(400, 400);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Title
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40)); // Link
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Tags
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // EnableAutoStartup
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Save button

            var titleLabel = new Label { Text = "Title:", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            titleTextBox = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(titleLabel, 0, 0);
            layout.Controls.Add(titleTextBox, 1, 0);

            var linkLabel = new Label { Text = "Link:", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            linkTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                MaxLength = 3000
            };
            layout.Controls.Add(linkLabel, 0, 1);
            layout.Controls.Add(linkTextBox, 1, 1);

            var tagsLabel = new Label { Text = "Tags:", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            tagsTextBox = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(tagsLabel, 0, 2);
            layout.Controls.Add(tagsTextBox, 1, 2);

            var enableAutoStartupLabel = new Label { Text = "Enable Auto Startup:", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            enableAutoStartupCheckBox = new CheckBox { Dock = DockStyle.Left };
            layout.Controls.Add(enableAutoStartupLabel, 0, 3);
            layout.Controls.Add(enableAutoStartupCheckBox, 1, 3);

            saveButton = new Button { Text = "Save", Dock = DockStyle.Right, Width = 100 };
            saveButton.Click += SaveButton_Click;
            layout.Controls.Add(saveButton, 1, 4);

            this.Controls.Add(layout);
        }

        // Helper class for managing Windows Startup shortcuts
        public static class StartupManager
        {
            public static void AddToStartup(string exePath, string shortcutName)
            {
                string startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string shortcutPath = Path.Combine(startupPath, shortcutName + ".lnk");

                // var shell = new WshShell();
                // IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                // shortcut.TargetPath = exePath;
                // shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
                // shortcut.Save();
            }

            public static void RemoveFromStartup(string shortcutName)
            {
                string startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string shortcutPath = Path.Combine(startupPath, shortcutName + ".lnk");
                if (File.Exists(shortcutPath))
                    File.Delete(shortcutPath);
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            var title = titleTextBox.Text.Trim();
            var link = linkTextBox.Text.Trim();
            var tags = tagsTextBox.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(link))
            {
                MessageBox.Show("Both Title and Link are required.", "Validation Error");
                return;
            }

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open(); // Ensure the connection is open
                }

                string query;
                if (linkId.HasValue)
                {
                    query = "UPDATE Links SET Title = @Title, Link = @Link, Tags = @Tags, EnableAutoStartup = @EnableAutoStartup, LastUpdated = CURRENT_TIMESTAMP WHERE Id = @Id";
                }
                else
                {
                    query = "INSERT INTO Links (Title, Link, Tags, EnableAutoStartup, LastUpdated) VALUES (@Title, @Link, @Tags, @EnableAutoStartup, CURRENT_TIMESTAMP)";
                }

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Title", title);
                    command.Parameters.AddWithValue("@Link", link);
                    command.Parameters.AddWithValue("@Tags", tags);
                    command.Parameters.AddWithValue("@EnableAutoStartup", enableAutoStartupCheckBox.Checked ? 1 : 0);
                    if (linkId.HasValue)
                    {
                        command.Parameters.AddWithValue("@Id", linkId.Value);
                    }

                    command.ExecuteNonQuery();
                }

                // Add or remove from startup based on EnableAutoStartup
                if (enableAutoStartupCheckBox.Checked)
                {
                    if (File.Exists(link)) // Only add if the file exists
                        StartupManager.AddToStartup(link, title);
                }
                else
                {
                    StartupManager.RemoveFromStartup(title);
                }

                // Ensure event is raised on the UI thread to avoid cross-thread exceptions
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => LinksEvents.RaiseLinksChanged()));
                }
                else
                {
                    LinksEvents.RaiseLinksChanged();
                }

                this.DialogResult = DialogResult.OK; // Indicate success
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving link: {ex.Message}", "Error");
            }
        }
    }
}