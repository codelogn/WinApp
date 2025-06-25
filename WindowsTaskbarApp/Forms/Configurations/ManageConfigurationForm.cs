using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsTaskbarApp.Services.Database;

namespace WindowsTaskbarApp.Forms.Configurations
{
    public partial class ManageConfigurationForm : Form
    {
        private SQLiteConnection connection;
        private DataGridView configGridView;
        private Button addButton;

        public ManageConfigurationForm()
        {
            InitializeComponent();
            this.Load += async (sender, e) =>
            {
                await InitializeDatabaseAsync();
                await LoadConfigurationsAsync();
            };
        }

        private void InitializeComponent()
        {
            this.configGridView = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
            this.addButton = new Button
            {
                Text = "Add Configuration",
                Dock = DockStyle.Top,
                BackColor = Color.DarkBlue,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            this.addButton.Click += AddButton_Click;

            configGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "Id", DataPropertyName = "Id", Visible = false });
            configGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Name", DataPropertyName = "Name" });
            configGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Key", HeaderText = "Key", DataPropertyName = "Key" });
            configGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Value", HeaderText = "Value", DataPropertyName = "Value" });

            var editButtonColumn = new DataGridViewButtonColumn { Name = "Edit", HeaderText = "Edit", Text = "Edit", UseColumnTextForButtonValue = true };
            configGridView.Columns.Add(editButtonColumn);
            var deleteButtonColumn = new DataGridViewButtonColumn { Name = "Delete", HeaderText = "Delete", Text = "Delete", UseColumnTextForButtonValue = true };
            configGridView.Columns.Add(deleteButtonColumn);

            configGridView.CellContentClick += ConfigGridView_CellContentClick;

            this.Controls.Add(configGridView);
            this.Controls.Add(addButton);
            this.Text = "Manage Configurations";
            this.Size = new Size(600, 400);
        }

        private async Task InitializeDatabaseAsync()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["AllInOneDb"].ConnectionString;
            connection = new SQLiteConnection(connectionString);
            await connection.OpenAsync();
        }

        private async Task LoadConfigurationsAsync()
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Configurations";
            var adapter = new SQLiteDataAdapter(command);
            var dataTable = new DataTable();
            await Task.Run(() => adapter.Fill(dataTable));
            configGridView.Invoke((Action)(() => { configGridView.DataSource = dataTable; }));
        }

        private async void AddButton_Click(object sender, EventArgs e)
        {
            using (var detailsForm = new ManageConfigurationDetailsForm(connection))
            {
                detailsForm.ConfigurationSaved += async (s, args) => { await LoadConfigurationsAsync(); };
                detailsForm.ShowDialog();
            }
        }

        private async void ConfigGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var columnName = configGridView.Columns[e.ColumnIndex].Name;
            if (columnName == "Edit")
            {
                using (var detailsForm = new ManageConfigurationDetailsForm(connection))
                {
                    detailsForm.Id = int.TryParse(configGridView.Rows[e.RowIndex].Cells["Id"].Value?.ToString(), out var parsedId) ? parsedId : (int?)null;
                    detailsForm.Name = configGridView.Rows[e.RowIndex].Cells["Name"].Value?.ToString();
                    detailsForm.Key = configGridView.Rows[e.RowIndex].Cells["Key"].Value?.ToString();
                    detailsForm.Value = configGridView.Rows[e.RowIndex].Cells["Value"].Value?.ToString();
                    detailsForm.ConfigurationSaved += async (s, args) => { await LoadConfigurationsAsync(); };
                    detailsForm.ShowDialog();
                }
            }
            else if (columnName == "Delete")
            {
                var id = configGridView.Rows[e.RowIndex].Cells["Id"].Value?.ToString();
                if (MessageBox.Show("Are you sure you want to delete this configuration?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "DELETE FROM Configurations WHERE Id = @id";
                        command.Parameters.AddWithValue("@id", id);
                        await Task.Run(() => command.ExecuteNonQuery());
                    }
                    await LoadConfigurationsAsync();
                }
            }
        }
    }
}
