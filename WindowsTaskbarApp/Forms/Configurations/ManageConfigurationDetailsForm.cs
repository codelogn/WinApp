using System;
using System.ComponentModel;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsTaskbarApp.Forms.Configurations
{
    public partial class ManageConfigurationDetailsForm : Form
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int? Id { get; set; }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Name { get => nameTextBox.Text; set => nameTextBox.Text = value; }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Key { get => keyTextBox.Text; set => keyTextBox.Text = value; }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Value { get => valueTextBox.Text; set => valueTextBox.Text = value; }
        private SQLiteConnection connection;
        private TextBox keyTextBox;
        private TextBox valueTextBox;
        private TextBox nameTextBox;
        private Button saveButton;
        public event EventHandler ConfigurationSaved;

        public ManageConfigurationDetailsForm(SQLiteConnection dbConnection)
        {
            connection = dbConnection;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(20),
                AutoSize = false,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90)); // Fixed width for labels
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Fill for fields
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));

            var nameLabel = new Label { Text = "Name:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill };
            nameTextBox = new TextBox { Dock = DockStyle.Fill, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(nameLabel, 0, 0);
            layout.Controls.Add(nameTextBox, 1, 0);

            var keyLabel = new Label { Text = "Key:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill };
            keyTextBox = new TextBox { Dock = DockStyle.Fill, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(keyLabel, 0, 1);
            layout.Controls.Add(keyTextBox, 1, 1);

            var valueLabel = new Label { Text = "Value:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill };
            valueTextBox = new TextBox { Dock = DockStyle.Fill, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(valueLabel, 0, 2);
            layout.Controls.Add(valueTextBox, 1, 2);

            saveButton = new Button { Text = "Save", AutoSize = true, BackColor = Color.DarkBlue, ForeColor = Color.White, Font = new Font("Arial", 10, FontStyle.Bold) };
            saveButton.Click += SaveButton_Click;
            var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(10), AutoSize = true };
            buttonPanel.Controls.Add(saveButton);
            this.Controls.Add(layout);
            this.Controls.Add(buttonPanel);
            this.Text = "Configuration Details";
            this.Size = new Size(400, 200);
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private async void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Key))
                {
                    MessageBox.Show("Key is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                using (var command = connection.CreateCommand())
                {
                    if (Id.HasValue)
                    {
                        command.CommandText = "UPDATE Configurations SET Name = @name, Key = @key, Value = @value WHERE Id = @id";
                        command.Parameters.AddWithValue("@id", Id.Value);
                    }
                    else
                    {
                        command.CommandText = "INSERT INTO Configurations (Name, Key, Value) VALUES (@name, @key, @value)";
                    }
                    command.Parameters.AddWithValue("@name", Name);
                    command.Parameters.AddWithValue("@key", Key);
                    command.Parameters.AddWithValue("@value", Value);
                    await command.ExecuteNonQueryAsync();
                }
                ConfigurationSaved?.Invoke(this, EventArgs.Empty);
                MessageBox.Show("Configuration saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving the configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
