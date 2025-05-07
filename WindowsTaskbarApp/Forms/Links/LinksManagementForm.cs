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
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            this.Controls.Add(linksDataGridView);

            // Initialize Add Button
            addButton = new Button { Text = "Add", Dock = DockStyle.Bottom, Height = 40 };
            this.Controls.Add(addButton);

            // Event Handlers
            this.Load += LinksManagementForm_Load;
            addButton.Click += AddButton_Click;
        }

        private void LinksManagementForm_Load(object sender, EventArgs e)
        {
            // Initialize SQLite connection
            connection = new SQLiteConnection("Data Source=alerts.db;Version=3;");
            EnsureLinksTableExists();
            LoadLinksData();

            // Add Edit and Delete button columns to the right
            if (!linksDataGridView.Columns.Contains("EditButton"))
            {
                var editButtonColumn = new DataGridViewButtonColumn
                {
                    Name = "EditButton",
                    HeaderText = "Edit",
                    Text = "Edit",
                    UseColumnTextForButtonValue = true
                };
                linksDataGridView.Columns.Add(editButtonColumn); // Add after data binding
            }

            if (!linksDataGridView.Columns.Contains("DeleteButton"))
            {
                var deleteButtonColumn = new DataGridViewButtonColumn
                {
                    Name = "DeleteButton",
                    HeaderText = "Delete",
                    Text = "Delete",
                    UseColumnTextForButtonValue = true
                };
                linksDataGridView.Columns.Add(deleteButtonColumn); // Add after data binding
            }

            // Handle button clicks in the grid
            linksDataGridView.CellClick += LinksDataGridView_CellClick;
        }

        private void LinksDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (linksDataGridView.Columns[e.ColumnIndex].Name == "EditButton")
                {
                    // Handle Edit button click
                    var id = Convert.ToInt32(linksDataGridView.Rows[e.RowIndex].Cells["Id"].Value);
                    var title = linksDataGridView.Rows[e.RowIndex].Cells["Title"].Value.ToString();
                    var link = linksDataGridView.Rows[e.RowIndex].Cells["Link"].Value.ToString();

                    using (var linkDetailsForm = new LinkDetailsForm(connection, id, title, link))
                    {
                        if (linkDetailsForm.ShowDialog() == DialogResult.OK)
                        {
                            LoadLinksData(); // Refresh the grid after editing
                        }
                    }
                }
                else if (linksDataGridView.Columns[e.ColumnIndex].Name == "DeleteButton")
                {
                    // Handle Delete button click
                    var idCell = linksDataGridView.Rows[e.RowIndex].Cells["Id"];
                    if (idCell != null && idCell.Value != null)
                    {
                        var id = idCell.Value.ToString();
                        var confirmResult = MessageBox.Show("Are you sure to delete this link?", "Confirm Delete", MessageBoxButtons.YesNo);

                        if (confirmResult == DialogResult.Yes)
                        {
                            try
                            {
                                connection.Open();
                                var command = new SQLiteCommand("DELETE FROM links WHERE Id = @Id", connection);
                                command.Parameters.AddWithValue("@Id", id);
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error deleting link: {ex.Message}");
                            }
                            finally
                            {
                                if (connection.State == ConnectionState.Open)
                                {
                                    connection.Close();
                                }
                            }

                            // Reload data after deletion
                            LoadLinksData();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Unable to retrieve the Id for the selected row.", "Error");
                    }
                }
            }
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
                // Clear the DataGridView to avoid conflicts
                linksDataGridView.DataSource = null;

                connection.Open();
                var query = "SELECT Id, Title, Link, LastUpdated FROM links";
                var adapter = new SQLiteDataAdapter(query, connection);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                // Bind the data to the DataGridView
                linksDataGridView.DataSource = dataTable;

                // Hide the Id column if not needed
                if (linksDataGridView.Columns["Id"] != null)
                {
                    linksDataGridView.Columns["Id"].Visible = false;
                }

                // Ensure Edit and Delete button columns are added to the right
                if (!linksDataGridView.Columns.Contains("EditButton"))
                {
                    var editButtonColumn = new DataGridViewButtonColumn
                    {
                        Name = "EditButton",
                        HeaderText = "Edit",
                        Text = "Edit",
                        UseColumnTextForButtonValue = true
                    };
                    linksDataGridView.Columns.Add(editButtonColumn);
                }

                if (!linksDataGridView.Columns.Contains("DeleteButton"))
                {
                    var deleteButtonColumn = new DataGridViewButtonColumn
                    {
                        Name = "DeleteButton",
                        HeaderText = "Delete",
                        Text = "Delete",
                        UseColumnTextForButtonValue = true
                    };
                    linksDataGridView.Columns.Add(deleteButtonColumn);
                }

                // Move Edit and Delete columns to the rightmost position
                linksDataGridView.Columns["EditButton"].DisplayIndex = linksDataGridView.Columns.Count - 2;
                linksDataGridView.Columns["DeleteButton"].DisplayIndex = linksDataGridView.Columns.Count - 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            using (var linkDetailsForm = new LinkDetailsForm(connection))
            {
                if (linkDetailsForm.ShowDialog() == DialogResult.OK)
                {
                    LoadLinksData(); // Refresh the grid after adding
                }
            }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            if (linksDataGridView.SelectedRows.Count > 0)
            {
                var id = Convert.ToInt32(linksDataGridView.SelectedRows[0].Cells["Id"].Value);
                var title = linksDataGridView.SelectedRows[0].Cells["Title"].Value.ToString();
                var link = linksDataGridView.SelectedRows[0].Cells["Link"].Value.ToString();

                using (var linkDetailsForm = new LinkDetailsForm(connection, id, title, link))
                {
                    if (linkDetailsForm.ShowDialog() == DialogResult.OK)
                    {
                        LoadLinksData(); // Refresh the grid after editing
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a link to edit.", "Edit Link");
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