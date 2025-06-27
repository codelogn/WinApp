using System;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsTaskbarApp.Utils;

namespace WindowsTaskbarApp.Forms.Alerts
{
    public partial class AlertDetailsForm : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int? Id { get; set; } // Add this property if it doesn't exist

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Topic { get => topicTextBox.Text; set => topicTextBox.Text = value; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string LastUpdatedTime { get => timeTextBox.Text; set => timeTextBox.Text = value; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BrowserRefreshMinutes { get => minutesTextBox.Text; set => minutesTextBox.Text = value; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Keywords { get => keywordsTextBox.Text; set => keywordsTextBox.Text = value; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Query { get => queryTextBox.Text; set => queryTextBox.Text = value; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string URL { get => urlTextBox.Text; set => urlTextBox.Text = value; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string HTTPMethod
        {
            get => txtHttpMethod.Text; // Use your TextBox name here
            set => txtHttpMethod.Text = value ?? string.Empty;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string HTTPHeader { get => headerTextBox.Text; set => headerTextBox.Text = value; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string HTTPBody { get => bodyTextBox.Text; set => bodyTextBox.Text = value; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ContentType
        {
            get => txtContentType.Text;
            set => txtContentType.Text = value ?? string.Empty;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Accept
        {
            get => txtAccept.Text;
            set => txtAccept.Text = value ?? string.Empty;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string UserAgent
        {
            get => txtUserAgent.Text;
            set => txtUserAgent.Text = value ?? string.Empty;
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Enabled { get => enabledCheckBox.Checked ? "Yes" : "No"; set => enabledCheckBox.Checked = value == "Yes"; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string LastTriggered { get => lastTriggeredTextBox.Text; set => lastTriggeredTextBox.Text = value; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ResponseType { get => responseTypeComboBox.Text; set => responseTypeComboBox.Text = value; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ExecutionType { get => executionTypeComboBox.Text; set => executionTypeComboBox.Text = value; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CheckIntervalMinutes { get => checkIntervalMinutesTextBox.Text; set => checkIntervalMinutesTextBox.Text = value; }

        private TextBox topicTextBox;
        private TextBox timeTextBox;
        private TextBox minutesTextBox;
        private TextBox keywordsTextBox;
        private TextBox queryTextBox;
        private TextBox urlTextBox;
        private ComboBox methodComboBox;
        private TextBox bodyTextBox;
        private CheckBox enabledCheckBox;
        private TextBox lastTriggeredTextBox;
        private ComboBox responseTypeComboBox;
        private ComboBox executionTypeComboBox;
        private Button saveButton;
        private Button testButton;
        private Button testWithKeywordsButton;
        private SQLiteConnection connection;
        private TextBox headerTextBox;
        private TextBox txtHttpMethod; // Added TextBox for HTTP Method
        private TextBox txtContentType;
        private TextBox txtAccept;
        private TextBox txtUserAgent;
        private TextBox checkIntervalMinutesTextBox;

        public event EventHandler AlertSaved;

        public AlertDetailsForm(SQLiteConnection dbConnection)
        {
            InitializeComponent();
            connection = dbConnection;
        }

        private void InitializeComponent()
        {
            var layoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                Padding = new Padding(10),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            // --- General Section (GroupBox) ---
            var generalGroup = new GroupBox
            {
                Text = "General",
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            var generalLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            generalLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            generalLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));

            AddFieldToLayout(generalLayout, "Enabled:", enabledCheckBox = new CheckBox { Dock = DockStyle.Left });
            AddFieldToLayout(generalLayout, "Last Triggered:", lastTriggeredTextBox = new TextBox { Dock = DockStyle.Fill, ReadOnly = true });
            AddFieldToLayout(generalLayout, "Last Updated:", timeTextBox = new TextBox { Dock = DockStyle.Fill });
            AddFieldToLayout(generalLayout, "Topic:", topicTextBox = new TextBox { Dock = DockStyle.Fill });
            AddFieldToLayout(generalLayout, "Browser Refresh Minutes:", minutesTextBox = new TextBox { Dock = DockStyle.Fill });
            AddFieldToLayout(generalLayout, "Check Interval Minutes:", checkIntervalMinutesTextBox = new TextBox { Dock = DockStyle.Fill });

            generalGroup.Controls.Add(generalLayout);
            layoutPanel.Controls.Add(generalGroup);

            // --- Request Section (GroupBox) ---
            var requestGroup = new GroupBox
            {
                Text = "Request",
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            var requestLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            requestLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            requestLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            
            AddFieldToLayout(requestLayout, "URL:", urlTextBox = new TextBox { Dock = DockStyle.Fill });
            AddFieldToLayout(requestLayout, "HTTP Method:", txtHttpMethod = new TextBox { Dock = DockStyle.Fill }); // Changed to TextBox
            AddFieldToLayout(requestLayout, "HTTP Header:", headerTextBox = new TextBox { Dock = DockStyle.Fill, Multiline = true, Height = 150 });
            AddFieldToLayout(requestLayout, "HTTP Body:", bodyTextBox = new TextBox { Dock = DockStyle.Fill, Multiline = true, Height = 100 });

            // New alerts table fields
            AddFieldToLayout(requestLayout, "Content-Type:", txtContentType = new TextBox { Dock = DockStyle.Fill });
            AddFieldToLayout(requestLayout, "Accept:", txtAccept = new TextBox { Dock = DockStyle.Fill });
            AddFieldToLayout(requestLayout, "User-Agent:", txtUserAgent = new TextBox { Dock = DockStyle.Fill });

            requestGroup.Controls.Add(requestLayout);
            layoutPanel.Controls.Add(requestGroup);

            // --- Response Section (GroupBox) ---
            var responseGroup = new GroupBox
            {
                Text = "Response",
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            var responseLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            responseLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            responseLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));

            AddFieldToLayout(responseLayout, "Keywords:", keywordsTextBox = new TextBox { Dock = DockStyle.Fill });
            AddFieldToLayout(responseLayout, "Query:", queryTextBox = new TextBox { Dock = DockStyle.Fill });
            AddFieldToLayout(responseLayout, "Response Type:", responseTypeComboBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            });
            responseTypeComboBox.Items.AddRange(new string[] { "JSON", "XML", "HTML" });
            AddFieldToLayout(responseLayout, "Execution Type:", executionTypeComboBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            });
            executionTypeComboBox.Items.AddRange(new string[] { "Win Alert", "Win Notification", "Win Browser", "Popup Box" });

            responseGroup.Controls.Add(responseLayout);
            layoutPanel.Controls.Add(responseGroup);

            // --- Buttons ---
            saveButton = new Button
            {
                Text = "Save",
                AutoSize = true,
                BackColor = Color.DarkBlue,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            saveButton.Click += SaveButton_Click;

            testButton = new Button
            {
                Text = "Test in Browser",
                AutoSize = true,
                BackColor = Color.Green,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            testButton.Click += TestButton_Click;

            testWithKeywordsButton = new Button
            {
                Text = "Test with Keywords",
                AutoSize = true,
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            testWithKeywordsButton.Click += TestWithKeywordsButton_Click;

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(10),
                AutoSize = true
            };
            buttonPanel.Controls.Add(saveButton);
            buttonPanel.Controls.Add(testButton);
            buttonPanel.Controls.Add(testWithKeywordsButton);

            // Add layoutPanel to a scrollable panel
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            scrollPanel.Controls.Add(layoutPanel);

            this.Controls.Add(scrollPanel);
            this.Controls.Add(buttonPanel);

            this.Text = "Alert Details";
            this.Size = new System.Drawing.Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;


        }

        private void AddFieldToLayout(TableLayoutPanel layout, string labelText, Control control)
        {
            var label = new Label
            {
                Text = labelText,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill
            };
            layout.Controls.Add(label);
            layout.Controls.Add(control);
        }

        private async void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                await Alert.SaveAlertAsync(
                    connection: connection,
                    topic: topicTextBox.Text,
                    lastUpdatedTime: DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    browserRefreshMinutes: minutesTextBox.Text,
                    checkIntervalMinutes: checkIntervalMinutesTextBox.Text,
                    keywords: keywordsTextBox.Text,
                    query: queryTextBox.Text,
                    url: urlTextBox.Text,
                    httpMethod: HTTPMethod,
                    httpBody: HTTPBody,
                    enabled: enabledCheckBox.Checked ? "Yes" : "No",
                    lastTriggered: LastTriggered, // Use the property, do not overwrite
                    responseType: responseTypeComboBox.Text,
                    httpHeader: HTTPHeader,
                    executionType: ExecutionType,
                    contentType: txtContentType.Text,
                    accept: txtAccept.Text,
                    userAgent: txtUserAgent.Text,
                    id: Id
                );

                // Raise the AlertSaved event
                AlertSaved?.Invoke(this, EventArgs.Empty);

                MessageBox.Show("Alert saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close(); // Close the form after saving
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving the alert: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TestButton_Click(object sender, EventArgs e)
        {
            // Use the URL field value to open the browser
            var url = urlTextBox.Text;

            // Open in the default browser
            // Browser.OpenInDefaultBrowser(url);

            // Open in an embedded browser window
            Browser.OpenInEmbeddedBrowser(url);
        }

        private async void TestWithKeywordsButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Fetch content using HttpHelper
                string content = await WindowsTaskbarApp.Services.HttpClient.HttpHelper.FetchContentAsync(
                    url: urlTextBox.Text,
                    httpMethod: HTTPMethod,
                    httpHeader: HTTPHeader,
                    httpBody: HTTPBody,
                    contentType: ContentType,
                    accept: Accept,
                    userAgent: UserAgent
                );

                // Show the first 1000 characters of the response in a message box
                string displayContent = string.IsNullOrEmpty(content) ? "No response or error." : (content.Length > 1000 ? content.Substring(0, 1000) + "..." : content);
                MessageBox.Show(displayContent, "HTTP Response", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching content: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}