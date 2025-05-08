using System;
using System.Data.SQLite;
using System.Windows.Forms;

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
            this.Size = new System.Drawing.Size(400, 400); // Increased height to accommodate the new checkbox

            var titleLabel = new Label { Text = "Title:", Left = 20, Top = 20, Width = 100 };
            titleTextBox = new TextBox { Left = 120, Top = 20, Width = 200 };

            var linkLabel = new Label { Text = "Link:", Left = 20, Top = 60, Width = 100 };
            linkTextBox = new TextBox
            {
                Left = 120,
                Top = 60,
                Width = 200,
                Height = 100, // Increased height for multi-line text area
                Multiline = true, // Enable multi-line
                ScrollBars = ScrollBars.Vertical, // Add a vertical scrollbar
                MaxLength = 3000 // Allow up to 3000 characters
            };

            var tagsLabel = new Label { Text = "Tags:", Left = 20, Top = 180, Width = 100 };
            tagsTextBox = new TextBox { Left = 120, Top = 180, Width = 200 };

            var enableAutoStartupLabel = new Label { Text = "Enable Auto Startup:", Left = 20, Top = 220, Width = 150 };
            enableAutoStartupCheckBox = new CheckBox { Left = 180, Top = 220 };

            saveButton = new Button { Text = "Save", Left = 120, Top = 270, Width = 100 }; // Adjusted position
            saveButton.Click += SaveButton_Click;

            this.Controls.Add(titleLabel);
            this.Controls.Add(titleTextBox);
            this.Controls.Add(linkLabel);
            this.Controls.Add(linkTextBox);
            this.Controls.Add(tagsLabel);
            this.Controls.Add(tagsTextBox);
            this.Controls.Add(enableAutoStartupLabel);
            this.Controls.Add(enableAutoStartupCheckBox);
            this.Controls.Add(saveButton);
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