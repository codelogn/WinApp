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
        public string LastUpdatedTime
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
        public string ResponseType
        {
            get => responseTypeComboBox.SelectedItem?.ToString();
            set => responseTypeComboBox.SelectedItem = value;
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
        private Button testWithKeywordsButton;
        private CheckBox enabledCheckBox;
        private DateTimePicker lastTriggeredPicker;
        private TableLayoutPanel layoutPanel;
        private ComboBox responseTypeComboBox;
        private SQLiteConnection connection;

        private bool isSaving = false;

        public AlertDetailsForm(SQLiteConnection dbConnection)
        {
            InitializeComponent();
            InitializeResponseTypeRadioButtons();
            connection = dbConnection; // Assign the passed connection to the field
        }

        public AlertDetailsForm(SQLiteConnection dbConnection, int? selectedId = null, string selectedTopic = null)
        {
            InitializeComponent();
            InitializeResponseTypeRadioButtons();
            connection = dbConnection; // Assign the passed connection to the field
            Id = selectedId; // Assign the selected ID
            Topic = selectedTopic; // Assign the selected topic
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
            AddFieldToLayout("URL:", urlTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                MaxLength = 2000 // Set the maximum character limit to 2000
            });
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
            AddFieldToLayout("Response Type:", responseTypeComboBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            });
            responseTypeComboBox.Items.AddRange(new string[] { "JSON", "XML", "HTML" });

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

            // Create Test with Keywords button
            testWithKeywordsButton = new Button
            {
                Text = "Test with Keywords",
                Dock = DockStyle.Top,
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            testWithKeywordsButton.Click += TestWithKeywordsButton_Click;
            layoutPanel.Controls.Add(testWithKeywordsButton, 1, layoutPanel.RowCount++);

            // Add the layout panel and buttons to the form
            var mainPanel = new Panel { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(layoutPanel);

            this.Controls.Add(mainPanel);
            this.Controls.Add(testButton);
            this.Controls.Add(saveButton);

            this.Text = "Alert Details";
            this.Size = new System.Drawing.Size(600, 400); // Set initial size
        }

        private void InitializeResponseTypeRadioButtons()
        {
            // Create a GroupBox to contain the radio buttons
            var responseTypeGroupBox = new GroupBox
            {
                Text = "Response Type",
                Location = new System.Drawing.Point(20, 220), // Adjust location as needed
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
            if (connection == null)
            {
                MessageBox.Show("Database connection is not initialized.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                await Alert.SaveAlertAsync(
                    connection: connection, // Use the connection field
                    topic: Topic,
                    lastUpdatedTime: LastUpdatedTime,
                    minutes: Minutes,
                    keywords: Keywords,
                    url: URL,
                    method: Method,
                    body: Body,
                    enabled: Enabled,
                    lastTriggered: LastTriggered,
                    responseType: ResponseType,
                    id: Id
                );

                MessageBox.Show("Alert saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving alert: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void TestButton_Click(object sender, EventArgs e)
        {
            var url = urlTextBox.Text; // Get the URL from the text box

            if (url.Length > 2000)
            {
                MessageBox.Show("The URL exceeds the maximum allowed length of 2000 characters.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult) || 
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                MessageBox.Show("The URL is not valid. Please enter a valid HTTP or HTTPS URL.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var encodedUrl = url; // Use the URL as-is
                Console.WriteLine($"Testing URL: {encodedUrl}");
                await UrlTester.TestUrlAsync(encodedUrl); // Use the URL as-is
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                MessageBox.Show($"Error testing URL({url}): {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void TestWithKeywordsButton_Click(object sender, EventArgs e)
        {
            var url = urlTextBox.Text;
            var keywords = keywordsTextBox.Text;

            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(keywords))
            {
                await UrlTester.TestUrlWithKeywordsAsync(url, keywords.Split(','));
            }
            else
            {
                MessageBox.Show("URL or Keywords are missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}