using System;
using System.ComponentModel;
using System.Windows.Forms;

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
            get => timePicker.Value.ToString("HH:mm");
            set => timePicker.Value = DateTime.TryParse(value, out var time) ? time : DateTime.Now;
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
            get => keywordsTextArea.Text;
            set => keywordsTextArea.Text = value;
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
            get => getRadioButton.Checked ? "GET" : postRadioButton.Checked ? "POST" : string.Empty;
            set
            {
                if (value == "GET") getRadioButton.Checked = true;
                else if (value == "POST") postRadioButton.Checked = true;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Body
        {
            get => bodyTextArea.Text;
            set => bodyTextArea.Text = value;
        }

        private TextBox topicTextBox;
        private DateTimePicker timePicker;
        private TextBox minutesTextBox;
        private TextBox keywordsTextArea;
        private TextBox urlTextBox;
        private RadioButton getRadioButton;
        private RadioButton postRadioButton;
        private TextBox bodyTextArea;
        private Button saveButton;

        public AlertDetailsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Alert Details";
            this.Size = new System.Drawing.Size(400, 500);

            var topicLabel = new Label { Text = "Topic:", Dock = DockStyle.Top, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            this.topicTextBox = new TextBox { PlaceholderText = "Enter Topic", Dock = DockStyle.Top };

            var timeLabel = new Label { Text = "Time:", Dock = DockStyle.Top, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            this.timePicker = new DateTimePicker { Format = DateTimePickerFormat.Time, Dock = DockStyle.Top };

            var minutesLabel = new Label { Text = "Minutes:", Dock = DockStyle.Top, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            this.minutesTextBox = new TextBox { PlaceholderText = "Enter Minutes", Dock = DockStyle.Top };

            var keywordsLabel = new Label { Text = "Keywords:", Dock = DockStyle.Top, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            this.keywordsTextArea = new TextBox { PlaceholderText = "Enter Keywords", Multiline = true, Height = 50, Dock = DockStyle.Top };

            var urlLabel = new Label { Text = "URL:", Dock = DockStyle.Top, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            this.urlTextBox = new TextBox { PlaceholderText = "Enter URL", Dock = DockStyle.Top };

            var methodLabel = new Label { Text = "Method:", Dock = DockStyle.Top, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            var radioPanel = new Panel { Dock = DockStyle.Top, Height = 30 };
            this.getRadioButton = new RadioButton { Text = "GET", Dock = DockStyle.Left, Width = 50 };
            this.postRadioButton = new RadioButton { Text = "POST", Dock = DockStyle.Left, Width = 50 };
            radioPanel.Controls.Add(this.getRadioButton);
            radioPanel.Controls.Add(this.postRadioButton);

            var bodyLabel = new Label { Text = "Body:", Dock = DockStyle.Top, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            this.bodyTextArea = new TextBox { PlaceholderText = "Enter Body", Multiline = true, Height = 100, Dock = DockStyle.Top };

            this.saveButton = new Button { Text = "Save", Dock = DockStyle.Bottom };
            this.saveButton.Click += SaveButton_Click;

            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.bodyTextArea);
            this.Controls.Add(bodyLabel);
            this.Controls.Add(radioPanel);
            this.Controls.Add(methodLabel);
            this.Controls.Add(this.urlTextBox);
            this.Controls.Add(urlLabel);
            this.Controls.Add(this.keywordsTextArea);
            this.Controls.Add(keywordsLabel);
            this.Controls.Add(this.minutesTextBox);
            this.Controls.Add(minutesLabel);
            this.Controls.Add(this.timePicker);
            this.Controls.Add(timeLabel);
            this.Controls.Add(this.topicTextBox);
            this.Controls.Add(topicLabel);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            this.Topic = topicTextBox.Text;
            this.Time = timePicker.Value.ToString("HH:mm");
            this.Minutes = minutesTextBox.Text;
            this.Keywords = keywordsTextArea.Text;
            this.URL = urlTextBox.Text;
            this.Method = getRadioButton.Checked ? "GET" : postRadioButton.Checked ? "POST" : string.Empty;
            this.Body = bodyTextArea.Text;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}