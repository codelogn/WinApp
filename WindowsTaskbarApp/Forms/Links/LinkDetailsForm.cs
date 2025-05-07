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
        private Button saveButton;

        public LinkDetailsForm(SQLiteConnection connection, int? linkId = null, string title = "", string link = "")
        {
            this.connection = connection;
            this.linkId = linkId;

            InitializeComponent();

            // Populate fields if editing
            titleTextBox.Text = title;
            linkTextBox.Text = link;
        }

        private void InitializeComponent()
        {
            this.Text = linkId.HasValue ? "Edit Link" : "Add Link";
            this.Size = new System.Drawing.Size(400, 200);

            var titleLabel = new Label { Text = "Title:", Left = 20, Top = 20, Width = 100 };
            titleTextBox = new TextBox { Left = 120, Top = 20, Width = 200 };

            var linkLabel = new Label { Text = "Link:", Left = 20, Top = 60, Width = 100 };
            linkTextBox = new TextBox { Left = 120, Top = 60, Width = 200 };

            saveButton = new Button { Text = "Save", Left = 120, Top = 100, Width = 100 };
            saveButton.Click += SaveButton_Click;

            this.Controls.Add(titleLabel);
            this.Controls.Add(titleTextBox);
            this.Controls.Add(linkLabel);
            this.Controls.Add(linkTextBox);
            this.Controls.Add(saveButton);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            var title = titleTextBox.Text.Trim();
            var link = linkTextBox.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(link))
            {
                MessageBox.Show("Both Title and Link are required.", "Validation Error");
                return;
            }

            try
            {
                connection.Open();

                SQLiteCommand command;
                if (linkId.HasValue)
                {
                    // Update existing record
                    command = new SQLiteCommand("UPDATE links SET Title = @Title, Link = @Link, LastUpdated = CURRENT_TIMESTAMP WHERE Id = @Id", connection);
                    command.Parameters.AddWithValue("@Id", linkId.Value);
                }
                else
                {
                    // Insert new record
                    command = new SQLiteCommand("INSERT INTO links (Title, Link, LastUpdated) VALUES (@Title, @Link, CURRENT_TIMESTAMP)", connection);
                }

                command.Parameters.AddWithValue("@Title", title);
                command.Parameters.AddWithValue("@Link", link);
                command.ExecuteNonQuery();

                this.DialogResult = DialogResult.OK; // Indicate success
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving link: {ex.Message}", "Error");
            }
            finally
            {
                connection.Close();
            }
        }
    }
}