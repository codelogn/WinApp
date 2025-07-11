using System;
using System.Windows.Forms;
using WindowsTaskbarApp.Jobs;

namespace WindowsTaskbarApp.Forms.Jobs // Renamed from Events
{
    public partial class JobsForm : Form // Renamed from EventsForm
    {
        private Timer logsRefreshTimer;
        private readonly BackgroundJobs backgroundJobs;
        private TextBox logsTextBox;
        private Button startButton;
        private Button stopButton;
        private Button clearLogsButton;
        private PictureBox runningIcon;

        public JobsForm(BackgroundJobs backgroundJobs) // Renamed constructor
        {
            InitializeComponent();
            this.backgroundJobs = backgroundJobs;

            logsRefreshTimer = new Timer();
            logsRefreshTimer.Interval = 1000; // 1 second
            logsRefreshTimer.Tick += LogsRefreshTimer_Tick;
            logsRefreshTimer.Start();
        }
        private void InitializeComponent()
        {
            this.Text = "Jobs"; // Renamed window title
            this.Size = new System.Drawing.Size(300, 200);

            logsTextBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Location = new System.Drawing.Point(20, 10),
                Size = new System.Drawing.Size(260, 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                ScrollBars = ScrollBars.Vertical
            };

            runningIcon = new PictureBox
            {
                AutoSize = true,
                SizeMode = PictureBoxSizeMode.AutoSize,
                Location = new System.Drawing.Point(20, 120),
                Visible = false,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Image = System.Drawing.SystemIcons.Warning.ToBitmap() // Placeholder icon
            };

            startButton = new Button
            {
                Text = "Start Job",
                Location = new System.Drawing.Point(50, 120),
                Size = new System.Drawing.Size(80, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            startButton.Click += StartButton_Click;

            stopButton = new Button
            {
                Text = "Stop Job",
                Location = new System.Drawing.Point(150, 120),
                Size = new System.Drawing.Size(80, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            stopButton.Click += StopButton_Click;

            clearLogsButton = new Button
            {
                Text = "Clear Logs",
                Location = new System.Drawing.Point(250, 120),
                Size = new System.Drawing.Size(80, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            clearLogsButton.Click += ClearLogsButton_Click;

            this.Controls.Add(logsTextBox);
            this.Controls.Add(runningIcon);
            this.Controls.Add(startButton);
            this.Controls.Add(stopButton);
            this.Controls.Add(clearLogsButton);
        }

        private void LogsRefreshTimer_Tick(object sender, EventArgs e)
        {
            logsTextBox.Text = backgroundJobs.Logs;
        }

        private void UpdateJobControls()
        {
            if (backgroundJobs.IsRunning)
            {
                startButton.Enabled = false;
                stopButton.Enabled = true;
                runningIcon.Visible = true;
            }
            else
            {
                startButton.Enabled = true;
                stopButton.Enabled = false;
                runningIcon.Visible = false;
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            backgroundJobs.Start();
            UpdateJobControls();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            backgroundJobs.Stop();
            UpdateJobControls();
        }

        private void ClearLogsButton_Click(object sender, EventArgs e)
        {
            backgroundJobs.ClearLogs();
            if (logsTextBox != null)
                logsTextBox.Text = string.Empty;
        }

        private void JobsForm_Load(object sender, EventArgs e) // Renamed event handler
        {
            UpdateJobControls();
        }
    }
}

