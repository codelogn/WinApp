using System;
using System.Data;
using System.Data.SQLite;
using System.Net.Http;
using System.Windows.Forms;
using System.Drawing;
using WindowsTaskbarApp.Utils; 

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
            this.addButton = new Button
            {
                Text = "Add Alert",
                Dock = DockStyle.Top,
                BackColor = Color.DarkBlue,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            this.addButton.Click += AddButton_Click;

            // Add data columns
            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "Id",
                DataPropertyName = "Id",
                Visible = true // Make the Id column visible
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Topic",
                HeaderText = "Topic",
                DataPropertyName = "Topic"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Time",
                HeaderText = "Time",
                DataPropertyName = "Time"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Minutes",
                HeaderText = "Minutes",
                DataPropertyName = "Minutes"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Keywords",
                HeaderText = "Keywords",
                DataPropertyName = "Keywords"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "URL",
                HeaderText = "URL",
                DataPropertyName = "URL"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Method",
                HeaderText = "Method",
                DataPropertyName = "Method"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Body",
                HeaderText = "Body",
                DataPropertyName = "Body"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Enabled",
                HeaderText = "Enabled",
                DataPropertyName = "Enabled"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "LastTriggered",
                HeaderText = "Last Triggered",
                DataPropertyName = "LastTriggered"
            });

            // Add button columns
            var editButtonColumn = new DataGridViewButtonColumn
            {
                Name = "Edit",
                HeaderText = "Edit",
                Text = "Edit",
                UseColumnTextForButtonValue = true
            };
            alertsGridView.Columns.Add(editButtonColumn);

            var deleteButtonColumn = new DataGridViewButtonColumn
            {
                Name = "Delete",
                HeaderText = "Delete",
                Text = "Delete",
                UseColumnTextForButtonValue = true
            };
            alertsGridView.Columns.Add(deleteButtonColumn);

            var testButtonColumn = new DataGridViewButtonColumn
            {
                Name = "Test",
                HeaderText = "Test",
                Text = "Test",
                UseColumnTextForButtonValue = true
            };
            alertsGridView.Columns.Add(testButtonColumn);

            // Add the CellPainting event handler
            alertsGridView.CellPainting += AlertsGridView_CellPainting;

            // Add the CellContentClick event handler
            alertsGridView.CellContentClick += AlertsGridView_CellContentClick;

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
                    Body TEXT,
                    Enabled TEXT DEFAULT 'Yes', -- New field for Enabled (Yes/No)
                    LastTriggered TEXT -- New field for Last Triggered (Datetime)
                );
            ";
            command.ExecuteNonQuery();
        }

        private void LoadAlerts()
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Topic, Time, Minutes, Keywords, URL, Method, Body, Enabled, LastTriggered
                FROM Alerts"; // Include all columns

            var adapter = new SQLiteDataAdapter(command);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);

            alertsGridView.DataSource = dataTable; // Bind the data source
        }

        private async void AddButton_Click(object sender, EventArgs e)
        {
            using (var detailsForm = new AlertDetailsForm())
            {
                if (detailsForm.ShowDialog() == DialogResult.OK)
                {
                    // Refresh the grid to show the new record
                    LoadAlerts();
                    alertsGridView.Refresh();
                }
            }
        }

        private async void AlertsGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Ensure a valid row is clicked
            {
                var idCell = alertsGridView.Rows[e.RowIndex].Cells["Id"];
                if (idCell == null || idCell.Value == null)
                {
                    MessageBox.Show("The 'Id' column is missing or has no value for this row.");
                    return;
                }

                var id = Convert.ToInt32(idCell.Value);

                // Handle the "Test" button click
                if (alertsGridView.Columns[e.ColumnIndex] is DataGridViewButtonColumn testColumn &&
                    testColumn.Name == "Test")
                {
                    var url = alertsGridView.Rows[e.RowIndex].Cells["URL"].Value?.ToString();
                    if (string.IsNullOrEmpty(url))
                    {
                        MessageBox.Show("URL is empty.");
                        return;
                    }

                    await UrlTester.TestUrlAsync(url); // Call the shared utility method
                }

                // Handle the "Edit" button click
                else if (alertsGridView.Columns[e.ColumnIndex] is DataGridViewButtonColumn editColumn &&
                         editColumn.Name == "Edit")
                {
                    using (var detailsForm = new AlertDetailsForm())
                    {
                        // Pass the selected record's data to the form
                        detailsForm.Id = Convert.ToInt32(alertsGridView.Rows[e.RowIndex].Cells["Id"].Value);
                        detailsForm.Topic = alertsGridView.Rows[e.RowIndex].Cells["Topic"].Value?.ToString();
                        detailsForm.Time = alertsGridView.Rows[e.RowIndex].Cells["Time"].Value?.ToString();
                        detailsForm.Minutes = alertsGridView.Rows[e.RowIndex].Cells["Minutes"].Value?.ToString();
                        detailsForm.Keywords = alertsGridView.Rows[e.RowIndex].Cells["Keywords"].Value?.ToString();
                        detailsForm.URL = alertsGridView.Rows[e.RowIndex].Cells["URL"].Value?.ToString();
                        detailsForm.Method = alertsGridView.Rows[e.RowIndex].Cells["Method"].Value?.ToString();
                        detailsForm.Body = alertsGridView.Rows[e.RowIndex].Cells["Body"].Value?.ToString();
                        detailsForm.Enabled = alertsGridView.Rows[e.RowIndex].Cells["Enabled"].Value?.ToString();
                        detailsForm.LastTriggered = alertsGridView.Rows[e.RowIndex].Cells["LastTriggered"].Value?.ToString();

                        if (detailsForm.ShowDialog() == DialogResult.OK)
                        {
                            // Refresh the grid to show the updated record
                            LoadAlerts();
                            alertsGridView.Refresh();
                        }
                    }
                }

                // Handle the "Delete" button click
                else if (alertsGridView.Columns[e.ColumnIndex] is DataGridViewButtonColumn deleteColumn &&
                         deleteColumn.Name == "Delete")
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

        private void AlertsGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var column = alertsGridView.Columns[e.ColumnIndex];

                // Check if the column is a button column
                if (column is DataGridViewButtonColumn)
                {
                    e.PaintBackground(e.ClipBounds, true);

                    // Set button colors
                    Color buttonBackColor = Color.DarkBlue;
                    Color buttonForeColor = Color.White;

                    if (column.Name == "Edit")
                    {
                        buttonBackColor = Color.LightBlue;
                    }
                    else if (column.Name == "Delete")
                    {
                        buttonBackColor = Color.Red;
                    }
                    else if (column.Name == "Test")
                    {
                        buttonBackColor = Color.Green;
                    }

                    // Draw the button
                    using (Brush backBrush = new SolidBrush(buttonBackColor))
                    using (Brush foreBrush = new SolidBrush(buttonForeColor))
                    {
                        // Fill the button background
                        e.Graphics.FillRectangle(backBrush, e.CellBounds);

                        // Draw the button text
                        var text = column.HeaderText;
                        if (column is DataGridViewButtonColumn buttonColumn)
                        {
                            text = buttonColumn.Text;
                        }

                        var textSize = e.Graphics.MeasureString(text, e.CellStyle.Font);
                        var textX = e.CellBounds.Left + (e.CellBounds.Width - textSize.Width) / 2;
                        var textY = e.CellBounds.Top + (e.CellBounds.Height - textSize.Height) / 2;

                        e.Graphics.DrawString(text, e.CellStyle.Font, foreBrush, new PointF(textX, textY));
                    }

                    e.Handled = true; // Prevent default painting
                }
            }
        }
    }
}