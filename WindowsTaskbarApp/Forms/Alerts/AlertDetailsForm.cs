using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Data.SQLite; // For SQLiteConnection
using WindowsTaskbarApp.Utils; // For the Alerts utility class

namespace WindowsTaskbarApp.Forms.Alerts
{
    public partial class AlertDetailsForm : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Topic
        {
            get => topicTextBox.Text;
            set => topicTextBox.Text = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Time
        {
            get => timeTextBox.Text;
            set => timeTextBox.Text = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Minutes
        {
            get => minutesTextBox.Text;
            set => minutesTextBox.Text = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Keywords
        {
            get => keywordsTextBox.Text;
            set => keywordsTextBox.Text = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string URL
        {
            get => urlTextBox.Text;
            set => urlTextBox.Text = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Method
        {
            get => methodComboBox.Text;
            set => methodComboBox.Text = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Body
        {
            get => bodyTextBox.Text;
            set => bodyTextBox.Text = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Enabled
        {
            get => enabledCheckBox.Checked ? "Yes" : "No";
            set => enabledCheckBox.Checked = value == "Yes";
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string LastTriggered
        {
            get => lastTriggeredPicker.Value.ToString("yyyy-MM-dd HH:mm:ss");
            set => lastTriggeredPicker.Value = DateTime.TryParse(value, out var date) ? date : DateTime.Now;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int? Id { get; set; } // Nullable to handle new records

        private TextBox topicTextBox;
        private TextBox timeTextBox;
        private TextBox minutesTextBox;
        private TextBox keywordsTextBox;
        private TextBox urlTextBox;
        private ComboBox methodComboBox;
        private TextBox bodyTextBox;
        private Button saveButton;
        private Button testButton;
        private CheckBox enabledCheckBox;
        private DateTimePicker lastTriggeredPicker;
        private TableLayoutPanel layoutPanel;

        public AlertDetailsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Create a TableLayoutPanel
            layoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 10,
                Padding = new Padding(10),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30)); // Label column
            layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70)); // Input field column

            // Add fields to the layout
            AddFieldToLayout("Topic:", topicTextBox = new TextBox { Dock = DockStyle.Fill });
            AddFieldToLayout("Time:", timeTextBox = new TextBox { Dock = DockStyle.Fill });
            AddFieldToLayout("Minutes:", minutesTextBox = new TextBox { Dock = DockStyle.Fill });
            AddFieldToLayout("Keywords:", keywordsTextBox = new TextBox { Dock = DockStyle.Fill });
            AddFieldToLayout("URL:", urlTextBox = new TextBox { Dock = DockStyle.Fill });
            AddFieldToLayout("Method:", methodComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList });
            methodComboBox.Items.AddRange(new string[] { "GET", "POST" });
            AddFieldToLayout("Body:", bodyTextBox = new TextBox { Dock = DockStyle.Fill, Multiline = true, Height = 100 });
            AddFieldToLayout("Enabled:", enabledCheckBox = new CheckBox { Dock = DockStyle.Left });
            AddFieldToLayout("Last Triggered:", lastTriggeredPicker = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd HH:mm:ss"
            });

            // Create Save button
            saveButton = new Button
            {
                Text = "Save",
                Dock = DockStyle.Top,
                BackColor = Color.DarkBlue, // Set background color
                ForeColor = Color.White,   // Set text color
                Font = new Font("Arial", 10, FontStyle.Bold) // Optional: Set font style
            };
            saveButton.Click += SaveButton_Click;

            // Create Test button
            testButton = new Button
            {
                Text = "Test URL",
                Dock = DockStyle.Top,
                BackColor = Color.Green, // Set background color
                ForeColor = Color.White, // Set text color
                Font = new Font("Arial", 10, FontStyle.Bold) // Optional: Set font style
            };
            testButton.Click += TestButton_Click;

            // Add the layout panel and buttons to the form
            var mainPanel = new Panel { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(layoutPanel);

            this.Controls.Add(mainPanel);
            this.Controls.Add(testButton);
            this.Controls.Add(saveButton);

            this.Text = "Alert Details";
            this.Size = new System.Drawing.Size(600, 400); // Set initial size
        }

        // Helper method to add a field to the layout
        private void AddFieldToLayout(string labelText, Control control)
        {
            var label = new Label
            {
                Text = labelText,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill
            };
            layoutPanel.Controls.Add(label);
            layoutPanel.Controls.Add(control);
        }

        private async void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var connection = new SQLiteConnection("Data Source=alerts.db;Version=3;"))
                {
                    connection.Open(); // Ensure the connection is open

                    await Alert.SaveAlertAsync(
                        connection: connection,
                        topic: topicTextBox.Text,
                        time: timeTextBox.Text,
                        minutes: minutesTextBox.Text,
                        keywords: keywordsTextBox.Text,
                        url: urlTextBox.Text,
                        method: methodComboBox.Text,
                        body: bodyTextBox.Text,
                        enabled: enabledCheckBox.Checked ? "Yes" : "No",
                        lastTriggered: lastTriggeredPicker.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                        id: Id // Pass the Id if it exists
                    );

                    MessageBox.Show("Alert saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Set DialogResult to OK to indicate success
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving alert: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void TestButton_Click(object sender, EventArgs e)
        {
            var url = urlTextBox.Text; // Get the URL from the text box
            await UrlTester.TestUrlAsync(url); // Call the centralized logic
        }
    }
}