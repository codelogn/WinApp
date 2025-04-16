using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

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
            InitializeDatabase();
            LoadAlerts();
        }

        private void InitializeComponent()
        {
            this.alertsGridView = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
            this.addButton = new Button { Text = "Add Alert", Dock = DockStyle.Top };

            this.addButton.Click += AddButton_Click;
            this.alertsGridView.CellContentClick += AlertsGridView_CellContentClick;

            // Configure DataGridView columns
            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "Id", // Explicitly set the Name property
                HeaderText = "Id", 
                DataPropertyName = "Id", 
                Visible = true // Set to false if you don't want to display it
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "Topic", // Explicitly set the Name property
                HeaderText = "Topic", 
                DataPropertyName = "Topic" 
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "Time", // Explicitly set the Name property
                HeaderText = "Time", 
                DataPropertyName = "Time" 
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "Minutes", // Explicitly set the Name property
                HeaderText = "Minutes", 
                DataPropertyName = "Minutes" 
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "Keywords", // Explicitly set the Name property
                HeaderText = "Keywords", 
                DataPropertyName = "Keywords" 
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "URL", // Explicitly set the Name property
                HeaderText = "URL", 
                DataPropertyName = "URL" 
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "Method", // Explicitly set the Name property
                HeaderText = "Method", 
                DataPropertyName = "Method" 
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "Body", // Explicitly set the Name property
                HeaderText = "Body", 
                DataPropertyName = "Body" 
            });

            alertsGridView.Columns.Add(new DataGridViewButtonColumn 
            { 
                Name = "Edit", // Explicitly set the Name property
                HeaderText = "Edit", 
                Text = "Edit", 
                UseColumnTextForButtonValue = true 
            });

            alertsGridView.Columns.Add(new DataGridViewButtonColumn 
            { 
                Name = "Delete", // Explicitly set the Name property
                HeaderText = "Delete", 
                Text = "Delete", 
                UseColumnTextForButtonValue = true 
            });

            this.Controls.Add(this.alertsGridView);
            this.Controls.Add(this.addButton);

            this.Text = "Manage Alerts";
            this.Size = new System.Drawing.Size(600, 400);
        }

        private void InitializeDatabase()
        {
            connection = new SQLiteConnection("Data Source=alerts.db;Version=3;");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Alerts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Topic TEXT NOT NULL, 
                    Time TEXT NOT NULL,
                    Minutes TEXT,
                    Keywords TEXT,
                    URL TEXT,
                    Method TEXT,
                    Body TEXT
                );
            ";
            command.ExecuteNonQuery();
        }

        private void LoadAlerts()
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Topic, Time, Minutes, Keywords, URL, Method, Body FROM Alerts"; // Explicitly include "Id"

            var adapter = new SQLiteDataAdapter(command);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);

            alertsGridView.DataSource = dataTable; // Bind the data source
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            using (var detailsForm = new AlertDetailsForm())
            {
                if (detailsForm.ShowDialog() == DialogResult.OK)
                {
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        INSERT INTO Alerts (Topic, Time, Minutes, Keywords, URL, Method, Body)
                        VALUES (@topic, @time, @minutes, @keywords, @url, @method, @body)";
                    command.Parameters.AddWithValue("@topic", detailsForm.Topic);
                    command.Parameters.AddWithValue("@time", detailsForm.Time);
                    command.Parameters.AddWithValue("@minutes", detailsForm.Minutes);
                    command.Parameters.AddWithValue("@keywords", detailsForm.Keywords);
                    command.Parameters.AddWithValue("@url", detailsForm.URL);
                    command.Parameters.AddWithValue("@method", detailsForm.Method);
                    command.Parameters.AddWithValue("@body", detailsForm.Body);
                    command.ExecuteNonQuery();

                    LoadAlerts();
                    alertsGridView.Refresh();
                }
            }
        }

        private void AlertsGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Get the Id of the selected row
                var idCell = alertsGridView.Rows[e.RowIndex].Cells["Id"];
                if (idCell == null || idCell.Value == null)
                {
                    MessageBox.Show("The 'Id' column is missing or has no value for this row.");
                    return;
                }

                var id = Convert.ToInt32(idCell.Value);

                // Check if the clicked column is the "Edit" button
                if (alertsGridView.Columns[e.ColumnIndex] is DataGridViewButtonColumn editColumn &&
                    editColumn.HeaderText == "Edit")
                {
                    using (var detailsForm = new AlertDetailsForm())
                    {
                        // Pre-fill the form with existing data
                        detailsForm.Topic = alertsGridView.Rows[e.RowIndex].Cells["Topic"].Value?.ToString() ?? string.Empty;
                        detailsForm.Time = alertsGridView.Rows[e.RowIndex].Cells["Time"].Value?.ToString() ?? string.Empty;
                        detailsForm.Minutes = alertsGridView.Rows[e.RowIndex].Cells["Minutes"].Value?.ToString() ?? string.Empty;
                        detailsForm.Keywords = alertsGridView.Rows[e.RowIndex].Cells["Keywords"].Value?.ToString() ?? string.Empty;
                        detailsForm.URL = alertsGridView.Rows[e.RowIndex].Cells["URL"].Value?.ToString() ?? string.Empty;
                        detailsForm.Method = alertsGridView.Rows[e.RowIndex].Cells["Method"].Value?.ToString() ?? string.Empty;
                        detailsForm.Body = alertsGridView.Rows[e.RowIndex].Cells["Body"].Value?.ToString() ?? string.Empty;

                        if (detailsForm.ShowDialog() == DialogResult.OK)
                        {
                            var command = connection.CreateCommand();
                            command.CommandText = @"
                                UPDATE Alerts
                                SET Topic = @topic, Time = @time, Minutes = @minutes, Keywords = @keywords,
                                    URL = @url, Method = @method, Body = @body
                                WHERE Id = @id";
                            command.Parameters.AddWithValue("@id", id);
                            command.Parameters.AddWithValue("@topic", detailsForm.Topic);
                            command.Parameters.AddWithValue("@time", detailsForm.Time);
                            command.Parameters.AddWithValue("@minutes", detailsForm.Minutes);
                            command.Parameters.AddWithValue("@keywords", detailsForm.Keywords);
                            command.Parameters.AddWithValue("@url", detailsForm.URL);
                            command.Parameters.AddWithValue("@method", detailsForm.Method);
                            command.Parameters.AddWithValue("@body", detailsForm.Body);
                            command.ExecuteNonQuery();

                            LoadAlerts();
                            alertsGridView.Refresh();
                        }
                    }
                }
                // Check if the clicked column is the "Delete" button
                else if (alertsGridView.Columns[e.ColumnIndex] is DataGridViewButtonColumn deleteColumn &&
                         deleteColumn.HeaderText == "Delete")
                {
                    var result = MessageBox.Show("Are you sure you want to delete this alert?", "Confirm Delete", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = "DELETE FROM Alerts WHERE Id = @id";
                        command.Parameters.AddWithValue("@id", id);
                        command.ExecuteNonQuery();

                        LoadAlerts();
                        alertsGridView.Refresh();
                    }
                }
            }
        }
    }
}