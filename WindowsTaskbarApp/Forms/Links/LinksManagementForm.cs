using System;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsTaskbarApp.Forms.Links
{
    public partial class LinksManagementForm : Form
    {
        private SQLiteConnection connection;
        private DataGridView linksDataGridView;
        private Button addButton;
        private TextBox searchTextBox;
        private Button searchButton;
        private DataTable linksTable;

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

            // Search box and button
            var searchPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight
            };
            searchTextBox = new TextBox
            {
                Width = 200,
                PlaceholderText = "Search..."
            };
            searchTextBox.KeyDown += SearchTextBox_KeyDown;
            searchButton = new Button
            {
                Text = "Search",
                AutoSize = true
            };
            searchButton.Click += SearchButton_Click;
            searchPanel.Controls.Add(searchTextBox);
            searchPanel.Controls.Add(searchButton);

            // Initialize DataGridView
            linksDataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            linksDataGridView.CellClick += LinksDataGridView_CellClick;
            linksDataGridView.ColumnHeaderMouseClick += LinksDataGridView_ColumnHeaderMouseClick;

            // Initialize Add Button
            addButton = new Button { Text = "Add", Dock = DockStyle.Bottom, Height = 40 };
            addButton.Click += AddButton_Click;

            // Use TableLayoutPanel for layout
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Search panel
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // DataGridView
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Add button

            mainLayout.Controls.Add(searchPanel, 0, 0);
            mainLayout.Controls.Add(linksDataGridView, 0, 1);
            mainLayout.Controls.Add(addButton, 0, 2);

            this.Controls.Add(mainLayout);
        }

        private async Task InitializeDatabaseAsync()
        {
            try
            {
                // Initialize SQLite connection
                string connectionString = ConfigurationManager.ConnectionStrings["AllInOneDb"].ConnectionString;
                connection = new SQLiteConnection(connectionString);
                await connection.OpenAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing database connection: {ex.Message}", "Error");
            }
        }

        private async Task LoadLinksDataAsync(string filter = null)
        {
            try
            {
                if (connection == null || connection.State != ConnectionState.Open)
                {
                    throw new InvalidOperationException("Database connection is not initialized or open.");
                }

                linksDataGridView.Invoke((Action)(() =>
                {
                    linksDataGridView.Columns.Clear();
                }));

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

                linksTable = dataTable;
                DataTable toDisplay = dataTable;
                if (!string.IsNullOrEmpty(filter))
                {
                    var filtered = dataTable.AsEnumerable()
                        .Where(row => row.ItemArray.Any(field => field != null && field.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0));
                    if (filtered.Any())
                        toDisplay = filtered.CopyToDataTable();
                    else
                        toDisplay = dataTable.Clone();
                }

                linksDataGridView.Invoke((Action)(() =>
                {
                    linksDataGridView.DataSource = toDisplay;

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

                    // Ensure DisplayIndex values are set correctly
                    if (linksDataGridView.Columns["LastUpdated"] != null && linksDataGridView.Columns["EditButton"] != null)
                    {
                        linksDataGridView.Columns["EditButton"].DisplayIndex = linksDataGridView.Columns["LastUpdated"].DisplayIndex + 1;
                    }

                    if (linksDataGridView.Columns["DeleteButton"] != null)
                    {
                        linksDataGridView.Columns["DeleteButton"].DisplayIndex = linksDataGridView.Columns.Count - 1;
                    }
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

        private void SearchButton_Click(object sender, EventArgs e)
        {
            string filter = searchTextBox.Text.Trim();
            _ = LoadLinksDataAsync(filter);
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                SearchButton_Click(sender, EventArgs.Empty);
            }
        }

        private void LinksDataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (linksTable == null) return;
            string columnName = linksDataGridView.Columns[e.ColumnIndex].Name;
            string sortDirection = "ASC";
            if (linksDataGridView.Tag is Tuple<string, string> lastSort && lastSort.Item1 == columnName && lastSort.Item2 == "ASC")
                sortDirection = "DESC";
            var sorted = sortDirection == "DESC"
                ? linksTable.AsEnumerable().OrderByDescending(row => row[columnName])
                : linksTable.AsEnumerable().OrderBy(row => row[columnName]);
            linksDataGridView.DataSource = sorted.CopyToDataTable();
            linksDataGridView.Tag = Tuple.Create(columnName, sortDirection);
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