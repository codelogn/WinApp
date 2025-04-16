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
        private TextBox titleTextBox;
        private DateTimePicker timePicker;
        private Button addButton, editButton, deleteButton;

        public AlertsForm()
        {
            InitializeComponent();
            InitializeDatabase();
            LoadAlerts();
        }

        private void InitializeComponent()
        {
            this.alertsGridView = new DataGridView { Dock = DockStyle.Top, Height = 200 };
            this.titleTextBox = new TextBox { PlaceholderText = "Enter Title", Dock = DockStyle.Top };
            this.timePicker = new DateTimePicker { Format = DateTimePickerFormat.Time, Dock = DockStyle.Top };

            this.addButton = new Button { Text = "Add", Dock = DockStyle.Left, Width = 75 };
            this.editButton = new Button { Text = "Edit", Dock = DockStyle.Left, Width = 75 };
            this.deleteButton = new Button { Text = "Delete", Dock = DockStyle.Left, Width = 75 };

            this.addButton.Click += addButton_Click;
            this.editButton.Click += editButton_Click;
            this.deleteButton.Click += deleteButton_Click;

            var buttonPanel = new Panel { Dock = DockStyle.Top, Height = 30 };
            buttonPanel.Controls.Add(this.addButton);
            buttonPanel.Controls.Add(this.editButton);
            buttonPanel.Controls.Add(this.deleteButton);

            this.Controls.Add(this.alertsGridView);
            this.Controls.Add(this.titleTextBox);
            this.Controls.Add(this.timePicker);
            this.Controls.Add(buttonPanel);

            this.Text = "Manage Alerts";
            this.Size = new System.Drawing.Size(400, 400);
        }

        private void InitializeDatabase()
        {
            connection = new SQLiteConnection("Data Source=alerts.db;Version=3;");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Alerts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Time TEXT NOT NULL
                );
            ";
            command.ExecuteNonQuery();
        }

        private void LoadAlerts()
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Alerts";

            var adapter = new SQLiteDataAdapter(command);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);

            alertsGridView.DataSource = dataTable;
        }

        private void AddAlert(string title, string time)
        {
            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Alerts (Title, Time) VALUES (@title, @time)";
            command.Parameters.AddWithValue("@title", title);
            command.Parameters.AddWithValue("@time", time);
            command.ExecuteNonQuery();

            LoadAlerts();
        }

        private void EditAlert(int id, string title, string time)
        {
            var command = connection.CreateCommand();
            command.CommandText = "UPDATE Alerts SET Title = @title, Time = @time WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@title", title);
            command.Parameters.AddWithValue("@time", time);
            command.ExecuteNonQuery();

            LoadAlerts();
        }

        private void DeleteAlert(int id)
        {
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Alerts WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();

            LoadAlerts();
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            var title = titleTextBox.Text;
            var time = timePicker.Value.ToString("HH:mm");

            AddAlert(title, time);
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            if (alertsGridView.SelectedRows.Count > 0)
            {
                var id = (int)alertsGridView.SelectedRows[0].Cells["Id"].Value;
                var title = titleTextBox.Text;
                var time = timePicker.Value.ToString("HH:mm");

                EditAlert(id, title, time);
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (alertsGridView.SelectedRows.Count > 0)
            {
                long id = (long)alertsGridView.SelectedRows[0].Cells["Id"].Value;
                int idAsInt = Convert.ToInt32(id);
                DeleteAlert(idAsInt);
            }
        }
    }
}