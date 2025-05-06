using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace WindowsTaskbarApp.Forms.Links
{
    public partial class LinksManagementForm : Form
    {
        private DataGridView linksDataGridView;
        private Button addButton;
        private Button editButton;
        private Button deleteButton;
        private SQLiteConnection connection;

        public LinksManagementForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Manage Links";
            this.Size = new System.Drawing.Size(600, 400);

            // Initialize DataGridView
            linksDataGridView = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 300,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            this.Controls.Add(linksDataGridView);

            // Initialize Buttons
            addButton = new Button { Text = "Add", Dock = DockStyle.Left, Width = 100 };
            editButton = new Button { Text = "Edit", Dock = DockStyle.Left, Width = 100 };
            deleteButton = new Button { Text = "Delete", Dock = DockStyle.Left, Width = 100 };

            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 50 };
            buttonPanel.Controls.Add(addButton);
            buttonPanel.Controls.Add(editButton);
            buttonPanel.Controls.Add(deleteButton);
            this.Controls.Add(buttonPanel);

            // Event Handlers
            this.Load += LinksManagementForm_Load;
            addButton.Click += AddButton_Click;
            editButton.Click += EditButton_Click;
            deleteButton.Click += DeleteButton_Click;
        }

        private void LinksManagementForm_Load(object sender, EventArgs e)
        {
            // Initialize SQLite connection
            connection = new SQLiteConnection("Data Source=alerts.db;Version=3;");
            EnsureLinksTableExists();
            LoadLinksData();
        }

        private void EnsureLinksTableExists()
        {
            try
            {
                connection.Open();
                var createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS links (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Link TEXT NOT NULL,
                        LastUpdated DATETIME DEFAULT CURRENT_TIMESTAMP
                    );";
                var command = new SQLiteCommand(createTableQuery, connection);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error ensuring table exists: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        private void LoadLinksData()
        {
            try
            {
                connection.Open();
                var query = "SELECT Id, Title, Link, LastUpdated FROM links";
                var adapter = new SQLiteDataAdapter(query, connection);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);
                linksDataGridView.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var title = Prompt.ShowDialog("Enter Title:", "Add Link");
            var link = Prompt.ShowDialog("Enter Link:", "Add Link");

            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(link))
            {
                try
                {
                    connection.Open();
                    var command = new SQLiteCommand("INSERT INTO links (Title, Link, LastUpdated) VALUES (@Title, @Link, CURRENT_TIMESTAMP)", connection);
                    command.Parameters.AddWithValue("@Title", title);
                    command.Parameters.AddWithValue("@Link", link);
                    command.ExecuteNonQuery();
                    LoadLinksData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding link: {ex.Message}");
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            if (linksDataGridView.SelectedRows.Count > 0)
            {
                var id = linksDataGridView.SelectedRows[0].Cells["Id"].Value.ToString();
                var title = Prompt.ShowDialog("Edit Title:", "Edit Link", linksDataGridView.SelectedRows[0].Cells["Title"].Value.ToString());
                var link = Prompt.ShowDialog("Edit Link:", "Edit Link", linksDataGridView.SelectedRows[0].Cells["Link"].Value.ToString());

                if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(link))
                {
                    try
                    {
                        connection.Open();
                        var command = new SQLiteCommand("UPDATE links SET Title = @Title, Link = @Link, LastUpdated = CURRENT_TIMESTAMP WHERE Id = @Id", connection);
                        command.Parameters.AddWithValue("@Title", title);
                        command.Parameters.AddWithValue("@Link", link);
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();
                        LoadLinksData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error editing link: {ex.Message}");
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (linksDataGridView.SelectedRows.Count > 0)
            {
                var id = linksDataGridView.SelectedRows[0].Cells["Id"].Value.ToString();
                var confirmResult = MessageBox.Show("Are you sure to delete this link?", "Confirm Delete", MessageBoxButtons.YesNo);

                if (confirmResult == DialogResult.Yes)
                {
                    try
                    {
                        connection.Open();
                        var command = new SQLiteCommand("DELETE FROM links WHERE Id = @Id", connection);
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();
                        LoadLinksData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting link: {ex.Message}");
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }
    }

    // Helper class for input dialogs
    public static class Prompt
    {
        public static string ShowDialog(string text, string caption, string defaultValue = "")
        {
            var prompt = new Form { Width = 400, Height = 150, Text = caption };
            var textLabel = new Label { Left = 20, Top = 20, Text = text, Width = 350 };
            var textBox = new TextBox { Left = 20, Top = 50, Width = 350, Text = defaultValue };
            var confirmation = new Button { Text = "OK", Left = 270, Width = 100, Top = 80, DialogResult = DialogResult.OK };
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : string.Empty;
        }
    }
}