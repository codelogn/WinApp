using System;
using System.Data;
using System.Data.SQLite;
using System.Net.Http;
using System.Windows.Forms;
using System.Drawing;
using WindowsTaskbarApp.Utils; 
using System.Threading.Tasks;
using System.IO;

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
            this.Load += async (sender, e) =>
            {
                await InitializeDatabaseAsync();
                await LoadAlertsAsync();
                InitializeResponseTypeRadioButtons();
            };
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
                Name = "LastUpdatedTime",
                HeaderText = "Last Updated Time",
                DataPropertyName = "LastUpdatedTime" // Ensure this matches the database column name
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

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ResponseType",
                HeaderText = "Response Type",
                DataPropertyName = "ResponseType" // Ensure this matches the database column name
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

            var testWithKeywordsButtonColumn = new DataGridViewButtonColumn
            {
                Name = "TestWithKeywords",
                HeaderText = "Test with Keywords",
                Text = "Test with Keywords",
                UseColumnTextForButtonValue = true
            };
            alertsGridView.Columns.Add(testWithKeywordsButtonColumn);

            // Add the CellPainting event handler
            alertsGridView.CellPainting += AlertsGridView_CellPainting;

            // Add the CellContentClick event handler
            alertsGridView.CellContentClick += AlertsGridView_CellContentClick;

            this.Controls.Add(this.alertsGridView);
            this.Controls.Add(this.addButton);

            this.Text = "Manage Alerts";
            this.Size = new System.Drawing.Size(600, 400);
        }

        private async Task InitializeDatabaseAsync()
        {
            var dbPath = "alerts.db";

            // Create the database file if it doesn't exist
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            // Open the database connection
            connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            await connection.OpenAsync();

            // Create the schema
            await InitializeDatabaseSchemaAsync(connection);
        }

        private static async Task InitializeDatabaseSchemaAsync(SQLiteConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Alerts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Topic TEXT NOT NULL,
                    LastUpdatedTime TEXT NOT NULL, -- Replacing the old 'Time' column
                    Minutes TEXT NOT NULL,
                    Keywords TEXT,
                    URL TEXT NOT NULL,
                    Method TEXT NOT NULL,
                    Body TEXT,
                    Enabled TEXT NOT NULL,
                    LastTriggered TEXT,
                    ResponseType TEXT DEFAULT 'JSON' -- Default value for ResponseType
                );
            ";

            await Task.Run(() => command.ExecuteNonQuery());
        }

        private async Task LoadAlertsAsync()
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Alerts";

            var adapter = new SQLiteDataAdapter(command);
            var dataTable = new DataTable();

            await Task.Run(() => adapter.Fill(dataTable));

            alertsGridView.Invoke((Action)(() =>
            {
                alertsGridView.DataSource = dataTable;
            }));
        }

        private async void AddButton_Click(object sender, EventArgs e)
        {
            using (var detailsForm = new AlertDetailsForm(connection))
            {
                if (detailsForm.ShowDialog() == DialogResult.OK)
                {
                    await LoadAlertsAsync(); // Refresh the alerts after saving
                }
            }
        }

        private async void EditButton_Click(object sender, EventArgs e)
        {
            if (alertsGridView.SelectedRows.Count > 0)
            {
                var selectedId = Convert.ToInt32(alertsGridView.SelectedRows[0].Cells["Id"].Value);
                var selectedTopic = alertsGridView.SelectedRows[0].Cells["Topic"].Value?.ToString();

                using (var detailsForm = new AlertDetailsForm(connection, selectedId, selectedTopic))
                {
                    if (detailsForm.ShowDialog() == DialogResult.OK)
                    {
                        await LoadAlertsAsync(); // Refresh the grid after saving
                    }
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

                // Handle the "TestWithKeywords" button click
                else if (alertsGridView.Columns[e.ColumnIndex] is DataGridViewButtonColumn testWithKeywordsColumn &&
                         testWithKeywordsColumn.Name == "TestWithKeywords")
                {
                    var url = alertsGridView.Rows[e.RowIndex].Cells["URL"].Value?.ToString();
                    var keywords = alertsGridView.Rows[e.RowIndex].Cells["Keywords"].Value?.ToString();

                    if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(keywords))
                    {
                        await UrlTester.TestUrlWithKeywordsAsync(url, keywords.Split(',')); // Await the method
                    }
                    else
                    {
                        MessageBox.Show("URL or Keywords are missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                // Handle the "Edit" button click
                else if (alertsGridView.Columns[e.ColumnIndex] is DataGridViewButtonColumn editColumn &&
                         editColumn.Name == "Edit")
                {
                    using (var detailsForm = new AlertDetailsForm(connection))
                    {
                        // Pass the selected record's data to the form
                        detailsForm.Id = Convert.ToInt32(alertsGridView.Rows[e.RowIndex].Cells["Id"].Value);
                        detailsForm.Topic = alertsGridView.Rows[e.RowIndex].Cells["Topic"].Value?.ToString();
                        detailsForm.LastUpdatedTime = alertsGridView.Rows[e.RowIndex].Cells["LastUpdatedTime"].Value?.ToString(); // Fixed here
                        detailsForm.Minutes = alertsGridView.Rows[e.RowIndex].Cells["Minutes"].Value?.ToString();
                        detailsForm.Keywords = alertsGridView.Rows[e.RowIndex].Cells["Keywords"].Value?.ToString();
                        detailsForm.URL = alertsGridView.Rows[e.RowIndex].Cells["URL"].Value?.ToString();
                        detailsForm.Method = alertsGridView.Rows[e.RowIndex].Cells["Method"].Value?.ToString();
                        detailsForm.Body = alertsGridView.Rows[e.RowIndex].Cells["Body"].Value?.ToString();
                        detailsForm.Enabled = alertsGridView.Rows[e.RowIndex].Cells["Enabled"].Value?.ToString();
                        detailsForm.LastTriggered = alertsGridView.Rows[e.RowIndex].Cells["LastTriggered"].Value?.ToString();

                        if (detailsForm.ShowDialog() == DialogResult.OK)
                        {
                            var command = connection.CreateCommand();
                            command.CommandText = @"
                                UPDATE Alerts
                                SET Topic = @topic,
                                    LastUpdatedTime = @lastUpdatedTime,
                                    Minutes = @minutes,
                                    Keywords = @keywords,
                                    URL = @url,
                                    Method = @method,
                                    Body = @body,
                                    Enabled = @enabled,
                                    ResponseType = @responseType
                                WHERE Id = @id;
                            ";

                            command.Parameters.AddWithValue("@id", detailsForm.Id);
                            command.Parameters.AddWithValue("@topic", detailsForm.Topic);
                            command.Parameters.AddWithValue("@lastUpdatedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            command.Parameters.AddWithValue("@minutes", detailsForm.Minutes);
                            command.Parameters.AddWithValue("@keywords", detailsForm.Keywords);
                            command.Parameters.AddWithValue("@url", detailsForm.URL);
                            command.Parameters.AddWithValue("@method", detailsForm.Method);
                            command.Parameters.AddWithValue("@body", detailsForm.Body);
                            command.Parameters.AddWithValue("@enabled", detailsForm.Enabled);
                            command.Parameters.AddWithValue("@responseType", detailsForm.ResponseType);

                            await Task.Run(() => command.ExecuteNonQuery());

                            // Refresh the grid to show the updated record
                            await LoadAlertsAsync();
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
                        await Task.Run(() => command.ExecuteNonQuery());

                        // Refresh the grid to show the updated records
                        await LoadAlertsAsync();
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

        private void InitializeResponseTypeRadioButtons()
        {
            // Create a GroupBox to contain the radio buttons
            var responseTypeGroupBox = new GroupBox
            {
                Text = "Response Type",
                Location = new System.Drawing.Point(20, 320), // Adjust location as needed
                Size = new System.Drawing.Size(200, 100) // Adjust size as needed
            };

            // Create the radio buttons
            var jsonRadioButton = new RadioButton
            {
                Text = "JSON",
                Location = new System.Drawing.Point(10, 20),
                Checked = true // Default selection
            };

            var xmlRadioButton = new RadioButton
            {
                Text = "XML",
                Location = new System.Drawing.Point(10, 40)
            };

            var htmlRadioButton = new RadioButton
            {
                Text = "HTML",
                Location = new System.Drawing.Point(10, 60)
            };

            // Add the radio buttons to the GroupBox
            responseTypeGroupBox.Controls.Add(jsonRadioButton);
            responseTypeGroupBox.Controls.Add(xmlRadioButton);
            responseTypeGroupBox.Controls.Add(htmlRadioButton);

            // Add the GroupBox to the form
            this.Controls.Add(responseTypeGroupBox);
        }
    }
}