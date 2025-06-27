using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Configuration;

namespace WindowsTaskbarApp.Forms.Logs
{
    public class LogsViewerForm : Form
    {
        private DataGridView logsGridView;
        private TextBox searchTextBox;
        private Button searchButton;
        private DataTable logsTable;
        private string connectionString;

        public LogsViewerForm()
        {
            this.Text = "Logs Viewer";
            this.Size = new Size(1000, 500);
            connectionString = ConfigurationManager.ConnectionStrings["AllInOneDb"].ConnectionString;

            // Search box and button panel
            var searchPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight
            };
            searchTextBox = new TextBox
            {
                Width = 300,
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
            this.Controls.Add(searchPanel);

            // DataGridView
            logsGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                MultiSelect = false,
                AllowUserToOrderColumns = true,
                AllowUserToResizeRows = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                AutoGenerateColumns = true,
                EnableHeadersVisualStyles = true
            };
            logsGridView.ColumnHeaderMouseClick += LogsGridView_ColumnHeaderMouseClick;
            this.Controls.Add(logsGridView);

            LoadLogs();
        }

        private void LoadLogs(string filter = null)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var query = "SELECT Id, Timestamp, Level, Message, Source, Exception FROM Logs ORDER BY Timestamp DESC";
                using (var adapter = new SQLiteDataAdapter(query, connection))
                {
                    logsTable = new DataTable();
                    adapter.Fill(logsTable);
                    if (!string.IsNullOrEmpty(filter))
                    {
                        var filtered = logsTable.AsEnumerable()
                            .Where(row => row.ItemArray.Any(field => field != null && field.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0));
                        if (filtered.Any())
                            logsGridView.DataSource = filtered.CopyToDataTable();
                        else
                            logsGridView.DataSource = logsTable.Clone();
                    }
                    else
                    {
                        logsGridView.DataSource = logsTable;
                    }
                }
            }
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            string filter = searchTextBox.Text.Trim();
            LoadLogs(filter);
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                SearchButton_Click(sender, EventArgs.Empty);
            }
        }

        private void LogsGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (logsTable == null) return;
            string columnName = logsGridView.Columns[e.ColumnIndex].Name;
            string sortDirection = "ASC";
            if (logsGridView.Tag is Tuple<string, string> lastSort && lastSort.Item1 == columnName && lastSort.Item2 == "ASC")
                sortDirection = "DESC";
            OrderedEnumerableRowCollection<DataRow> sorted;
            if (sortDirection == "DESC")
                sorted = logsTable.AsEnumerable().OrderByDescending(row => row[columnName]);
            else
                sorted = logsTable.AsEnumerable().OrderBy(row => row[columnName]);
            logsGridView.DataSource = sorted.CopyToDataTable();
            logsGridView.Tag = Tuple.Create(columnName, sortDirection);
        }
    }
}
