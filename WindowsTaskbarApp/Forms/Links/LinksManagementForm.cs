using System;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsTaskbarApp.Forms.Links
{
    public partial class LinksManagementForm : Form
    {
        private SQLiteConnection connection;
        private DataGridView linksDataGridView;
        private Button addButton;

        public LinksManagementForm()
        {
            InitializeComponent();
            this.Load += async (sender, e) =>
            {
                await InitializeDatabaseAsync();
                await LoadLinksDataAsync();
            };
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
            linksDataGridView.CellClick += LinksDataGridView_CellClick; // Attach CellClick event
            this.Controls.Add(linksDataGridView);

            // Initialize Add Button
            addButton = new Button { Text = "Add", Dock = DockStyle.Bottom, Height = 40 };
            this.Controls.Add(addButton);

            // Event Handlers
            addButton.Click += AddButton_Click;
        }

        private async Task InitializeDatabaseAsync()
        {
            try
            {
                // Initialize SQLite connection
                connection = new SQLiteConnection("Data Source=alerts.db;Version=3;");
                await connection.OpenAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing database connection: {ex.Message}", "Error");
            }
        }

        private async Task LoadLinksDataAsync()
        {
            try
            {
                if (connection == null || connection.State != ConnectionState.Open)
                {
                    throw new InvalidOperationException("Database connection is not initialized or open.");
                }

                linksDataGridView.DataSource = null;

                var query = "SELECT Id, Title, Link, Tags, EnableAutoStartup, LastUpdated FROM Links";
                var adapter = new SQLiteDataAdapter(query, connection);
                var dataTable = new DataTable();

                await Task.Run(() => adapter.Fill(dataTable));

                foreach (DataRow row in dataTable.Rows)
                {
                    if (row["LastUpdated"] != DBNull.Value)
                    {
                        var utcTime = DateTime.Parse(row["LastUpdated"].ToString());
                        var localTime = utcTime.ToLocalTime();
                        row["LastUpdated"] = localTime.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                }

                linksDataGridView.Invoke((Action)(() =>
                {
                    linksDataGridView.DataSource = dataTable;

                    if (linksDataGridView.Columns["Id"] != null)
                    {
                        linksDataGridView.Columns["Id"].Visible = false;
                    }

                    // Add Edit and Delete button columns if not already added
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

                    // Move Edit and Delete buttons to the rightmost columns
                    linksDataGridView.Columns["EditButton"].DisplayIndex = linksDataGridView.Columns.Count - 2;
                    linksDataGridView.Columns["DeleteButton"].DisplayIndex = linksDataGridView.Columns.Count - 1;
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error");
            }
        }

        private async void AddButton_Click(object sender, EventArgs e)
        {
            using (var linkDetailsForm = new LinkDetailsForm(connection))
            {
                if (linkDetailsForm.ShowDialog() == DialogResult.OK)
                {
                    await LoadLinksDataAsync(); // Refresh the grid after adding
                }
            }
        }

        private async void LinksDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (linksDataGridView.Columns[e.ColumnIndex].Name == "EditButton")
                {
                    // Handle Edit button click
                    var id = Convert.ToInt32(linksDataGridView.Rows[e.RowIndex].Cells["Id"].Value);
                    var title = linksDataGridView.Rows[e.RowIndex].Cells["Title"].Value.ToString();
                    var link = linksDataGridView.Rows[e.RowIndex].Cells["Link"].Value.ToString();
                    var tags = linksDataGridView.Rows[e.RowIndex].Cells["Tags"].Value.ToString();
                    var enableAutoStartup = Convert.ToBoolean(linksDataGridView.Rows[e.RowIndex].Cells["EnableAutoStartup"].Value);

                    using (var linkDetailsForm = new LinkDetailsForm(connection, id, title, link, tags, enableAutoStartup))
                    {
                        if (linkDetailsForm.ShowDialog() == DialogResult.OK)
                        {
                            await LoadLinksDataAsync(); // Refresh the grid after editing
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
                                var command = new SQLiteCommand("DELETE FROM Links WHERE Id = @Id", connection);
                                command.Parameters.AddWithValue("@Id", id);
                                await command.ExecuteNonQueryAsync();

                                // Reload data after deletion
                                await LoadLinksDataAsync();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error deleting link: {ex.Message}");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Unable to retrieve the Id for the selected row.", "Error");
                    }
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Ensure the database connection is closed when the form is closed
            if (connection != null && connection.State == ConnectionState.Open)
            {
                connection.Close();
            }

            base.OnFormClosing(e);
        }
    }
}