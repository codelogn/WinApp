using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsTaskbarApp.Services.Database;
using WindowsTaskbarApp.Utils;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using System.Collections.Generic;

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
                Visible = false
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
                DataPropertyName = "LastUpdatedTime"
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
                Name = "Query",
                HeaderText = "Query",
                DataPropertyName = "Query"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "URL",
                HeaderText = "URL",
                DataPropertyName = "URL"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "HTTPMethod",
                HeaderText = "HTTP Method",
                DataPropertyName = "HTTPMethod"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "HTTPHeader",
                HeaderText = "HTTP Header",
                DataPropertyName = "HTTPHeader"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "HTTPBody",
                HeaderText = "HTTP Body",
                DataPropertyName = "HTTPBody"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Enabled",
                HeaderText = "Enabled",
                DataPropertyName = "Enabled"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ResponseType",
                HeaderText = "Response Type",
                DataPropertyName = "ResponseType"
            });

            alertsGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ExecutionType",
                HeaderText = "Execution Type",
                DataPropertyName = "ExecutionType"
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

            var openWebButtonColumn = new DataGridViewButtonColumn
            {
                Name = "OpenWeb",
                HeaderText = "Open Web",
                Text = "Open Web",
                UseColumnTextForButtonValue = true
            };
            alertsGridView.Columns.Add(openWebButtonColumn);

            alertsGridView.CellContentClick += AlertsGridView_CellContentClick;

            this.Controls.Add(this.alertsGridView);
            this.Controls.Add(this.addButton);

            this.Text = "Manage Alerts";
            this.Size = new System.Drawing.Size(800, 600);
        }

        private async Task InitializeDatabaseAsync()
        {
            // Initialize the database and tables
            DatabaseInitializer.Initialize();

            // Open the SQLite connection
            connection = new SQLiteConnection("Data Source=alerts.db;Version=3;");
            await connection.OpenAsync();
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
                detailsForm.AlertSaved += async (s, args) =>
                {
                    await LoadAlertsAsync(); // Refresh the grid view
                };

                detailsForm.ShowDialog();
            }
        }

        private async void AlertsGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var columnName = alertsGridView.Columns[e.ColumnIndex].Name;

            if (columnName == "Edit")
            {
                using (var detailsForm = new AlertDetailsForm(connection))
                {
                    // Populate fields
                    detailsForm.Id = int.TryParse(alertsGridView.Rows[e.RowIndex].Cells["Id"].Value?.ToString(), out var parsedId) ? parsedId : (int?)null;
                    detailsForm.Topic = alertsGridView.Rows[e.RowIndex].Cells["Topic"].Value?.ToString();
                    detailsForm.LastUpdatedTime = alertsGridView.Rows[e.RowIndex].Cells["LastUpdatedTime"].Value?.ToString();
                    detailsForm.Minutes = alertsGridView.Rows[e.RowIndex].Cells["Minutes"].Value?.ToString();
                    detailsForm.Keywords = alertsGridView.Rows[e.RowIndex].Cells["Keywords"].Value?.ToString();
                    detailsForm.Query = alertsGridView.Rows[e.RowIndex].Cells["Query"].Value?.ToString();
                    detailsForm.URL = alertsGridView.Rows[e.RowIndex].Cells["URL"].Value?.ToString();
                    detailsForm.HTTPMethod = alertsGridView.Rows[e.RowIndex].Cells["HTTPMethod"].Value?.ToString();
                    detailsForm.HTTPBody = alertsGridView.Rows[e.RowIndex].Cells["HTTPBody"].Value?.ToString();
                    detailsForm.Enabled = alertsGridView.Rows[e.RowIndex].Cells["Enabled"].Value?.ToString();
                    detailsForm.ResponseType = alertsGridView.Rows[e.RowIndex].Cells["ResponseType"].Value?.ToString();
                    detailsForm.ExecutionType = alertsGridView.Rows[e.RowIndex].Cells["ExecutionType"].Value?.ToString();
                    detailsForm.HTTPHeader = alertsGridView.Rows[e.RowIndex].Cells["HTTPHeader"].Value?.ToString();

                    // Subscribe to the AlertSaved event
                    detailsForm.AlertSaved += async (s, args) =>
                    {
                        await LoadAlertsAsync(); // Refresh the grid view
                    };

                    detailsForm.ShowDialog();
                }
            }
            else if (columnName == "Delete")
            {
                // Delete the selected alert
                var id = alertsGridView.Rows[e.RowIndex].Cells["Id"].Value?.ToString();
                if (MessageBox.Show("Are you sure you want to delete this alert?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "DELETE FROM Alerts WHERE Id = @id";
                            command.Parameters.AddWithValue("@id", id);

                            await Task.Run(() => command.ExecuteNonQuery());
                        }

                        await LoadAlertsAsync(); // Automatically refresh the grid view
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while deleting the alert: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else if (columnName == "Test")
            {
                try
                {
                    var url = alertsGridView.Rows[e.RowIndex].Cells["URL"].Value?.ToString();
                    var method = alertsGridView.Rows[e.RowIndex].Cells["HTTPMethod"].Value?.ToString();
                    var headers = alertsGridView.Rows[e.RowIndex].Cells["HTTPHeader"].Value?.ToString();
                    var body = alertsGridView.Rows[e.RowIndex].Cells["HTTPBody"].Value?.ToString();

                    if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(method))
                    {
                        MessageBox.Show("Invalid URL or HTTP Method.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var headerDict = new Dictionary<string, string>();
                    if (!string.IsNullOrEmpty(headers))
                    {
                        foreach (var header in headers.Split(';'))
                        {
                            var keyValue = header.Split(':');
                            if (keyValue.Length == 2)
                                headerDict[keyValue[0].Trim()] = keyValue[1].Trim();
                        }
                    }

                    // Send the HTTP request and display the response
                    var response = await HttpRestApi.SendRequestAsync(url, method, headerDict, body);
                    MessageBox.Show(response, "HTTP Response", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    // Catch any exception and display it in an alert box
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (columnName == "OpenWeb")
            {
                var url = alertsGridView.Rows[e.RowIndex].Cells["URL"].Value?.ToString();
                var minutesValue = alertsGridView.Rows[e.RowIndex].Cells["Minutes"].Value?.ToString();

                if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(minutesValue) || !int.TryParse(minutesValue, out var minutes))
                {
                    MessageBox.Show("Invalid URL or Minutes value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var browserForm = new FullScreenBrowserForm(url, minutes);
                browserForm.Show();
            }
        }
    }
}