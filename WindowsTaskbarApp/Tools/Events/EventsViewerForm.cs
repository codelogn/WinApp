using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Configuration;

namespace WindowsTaskbarApp.Tools.Events
{
    public class EventsViewerForm : Form
    {
        private DataGridView eventsGridView;
        private TextBox searchTextBox;
        private DataTable eventsTable;
        private string connectionString;

        public EventsViewerForm()
        {
            this.Text = "Events Viewer";
            this.Size = new Size(900, 500);
            connectionString = ConfigurationManager.ConnectionStrings["AllInOneDb"].ConnectionString;

            // Search box
            searchTextBox = new TextBox
            {
                Dock = DockStyle.Top,
                PlaceholderText = "Search..."
            };
            searchTextBox.TextChanged += SearchTextBox_TextChanged;
            this.Controls.Add(searchTextBox);

            // DataGridView
            eventsGridView = new DataGridView
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
            eventsGridView.ColumnHeaderMouseClick += EventsGridView_ColumnHeaderMouseClick;
            this.Controls.Add(eventsGridView);

            LoadEvents();
        }

        private void LoadEvents()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var query = "SELECT Id, EventName, EventContent, CreateDate, AlertId, SourceUrl, NotificationType FROM Events ORDER BY CreateDate DESC";
                using (var adapter = new SQLiteDataAdapter(query, connection))
                {
                    eventsTable = new DataTable();
                    adapter.Fill(eventsTable);
                    eventsGridView.DataSource = eventsTable;
                }
            }
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            if (eventsTable == null) return;
            string filter = searchTextBox.Text.Trim().Replace("'", "''");
            if (string.IsNullOrEmpty(filter))
            {
                eventsGridView.DataSource = eventsTable;
            }
            else
            {
                var filtered = eventsTable.AsEnumerable()
                    .Where(row => row.ItemArray.Any(field => field != null && field.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0));
                if (filtered.Any())
                    eventsGridView.DataSource = filtered.CopyToDataTable();
                else
                    eventsGridView.DataSource = eventsTable.Clone();
            }
        }

        private void EventsGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (eventsTable == null) return;
            string columnName = eventsGridView.Columns[e.ColumnIndex].Name;
            string sortDirection = "ASC";
            if (eventsGridView.Tag is Tuple<string, string> lastSort && lastSort.Item1 == columnName && lastSort.Item2 == "ASC")
                sortDirection = "DESC";
            var sorted = eventsTable.AsEnumerable().OrderBy(row => row[columnName]);
            if (sortDirection == "DESC")
                eventsGridView.DataSource = sorted.Reverse().CopyToDataTable();
            else
                eventsGridView.DataSource = sorted.CopyToDataTable();
            eventsGridView.Tag = Tuple.Create(columnName, sortDirection);
        }
    }
}
