using System;
using System.Drawing;
using System.Windows.Forms;
using WindowsTaskbarApp.Forms.Alerts;
using WindowsTaskbarApp.Jobs;

namespace WindowsTaskbarApp.Forms
{
    public partial class MainForm : Form
    {
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenu;
        private MenuStrip menuStrip;
        private Timer clockCountdownTimer;
        private CountdownOverlayForm overlayForm;
        private int remainingTime;
        private Timer fullClockCountdownTimer;
        private CountdownTimerBox countdownTimerBox;
        private ClockCountdownBox clockCountdownBox;
        private FullClockCountdownBox fullClockCountdownBox;
        private ToolStripMenuItem backgroundJobsMenuItem;
        private BackgroundJobs backgroundJobs;
        private TextBox statusTextBox;

        public MainForm()
        {
            // Initialize the form
            this.Text = "Main Application";
            this.Size = new Size(400, 300);

            // Create a MenuStrip
            menuStrip = new MenuStrip();

            // Add "File" menu
            var fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("Exit", null, (s, e) => Application.Exit());
            menuStrip.Items.Add(fileMenu);

            // Add "Tools" menu
            var toolsMenu = new ToolStripMenuItem("Tools");
            toolsMenu.DropDownItems.Add("Open Web", null, OpenWeb); // Moved from "File" to "Tools"
            toolsMenu.DropDownItems.Add("Text to Speech", null, OpenTextToSpeechForm); // Moved from "File" to "Tools"
            toolsMenu.DropDownItems.Add("Countdown Timer", null, OpenCountdownTimerForm);

            // Add "Clock Countdown" menu item
            var clockCountdownMenuItem = new ToolStripMenuItem("Clock Countdown");
            clockCountdownMenuItem.Click += ClockCountdownMenuItem_Click;
            toolsMenu.DropDownItems.Add(clockCountdownMenuItem);

            // Add "Full Clock Countdown" menu item
            var fullClockCountdownMenuItem = new ToolStripMenuItem("Full Clock Countdown");
            fullClockCountdownMenuItem.Click += FullClockCountdownMenuItem_Click;
            toolsMenu.DropDownItems.Add(fullClockCountdownMenuItem);

            // Add "Execution" menu item
            var executionMenuItem = new ToolStripMenuItem("Execution");
            executionMenuItem.Click += OpenExecutionForm;
            toolsMenu.DropDownItems.Add(executionMenuItem);

            toolsMenu.DropDownItems.Add("Alerts", null, OpenAlertsForm);

            // Add Background Jobs menu item
            backgroundJobsMenuItem = new ToolStripMenuItem("Background Jobs");
            backgroundJobsMenuItem.Click += BackgroundJobsMenuItem_Click;
            toolsMenu.DropDownItems.Add(backgroundJobsMenuItem); // Assuming toolsMenu is the Tools menu

            menuStrip.Items.Add(toolsMenu);

            // Add "Help" menu after "Tools"
            var helpMenu = new ToolStripMenuItem("Help");
            helpMenu.DropDownItems.Add("About", null, ShowAbout);
            menuStrip.Items.Add(helpMenu);

            // Add the MenuStrip to the form
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            // Create a context menu for the NotifyIcon
            contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Open", null, OnOpenClicked);
            contextMenu.Items.Add("Exit", null, OnExitClicked);

            // Create the NotifyIcon
            notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                ContextMenuStrip = contextMenu,
                Visible = true
            };

            // Initialize Clock Countdown Timer
            clockCountdownTimer = new Timer
            {
                Interval = 1000 // Check every second
            };
            clockCountdownTimer.Tick += ClockCountdownTimer_Tick;

            // Initialize the overlay form
            overlayForm = new CountdownOverlayForm();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            backgroundJobs = new BackgroundJobs();
            backgroundJobs.JobStatusUpdated += BackgroundJobs_JobStatusUpdated;
        }

        private void OpenWeb(object sender, EventArgs e)
        {
            // Create and show an instance of Form1
            var form1 = new Form1();
            form1.Show();
        }

        private void OpenTextToSpeechForm(object sender, EventArgs e)
        {
            var textToSpeechForm = new TextToSpeechForm();
            textToSpeechForm.Show();
        }

        private void OpenCountdownTimerForm(object sender, EventArgs e)
        {
            // Check if the box is null or disposed, and create a new instance if necessary
            if (countdownTimerBox == null || countdownTimerBox.IsDisposed)
            {
                countdownTimerBox = new CountdownTimerBox();
            }

            // Show the box and start the countdown
            countdownTimerBox.Show();
            countdownTimerBox.StartCountdown(5, 0); // Example: Start a 5-minute countdown
        }

        private void ShowAbout(object sender, EventArgs e)
        {
            MessageBox.Show("This is a sample Windows Forms application.", "About");
        }

        private void OnOpenClicked(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void OnExitClicked(object sender, EventArgs e)
        {
            notifyIcon.Dispose();
            Application.Exit();
        }

        private void ClockCountdownMenuItem_Click(object sender, EventArgs e)
        {
            clockCountdownBox ??= new ClockCountdownBox();
            clockCountdownBox.Show();
            clockCountdownBox.StartCountdownToNextMark();
        }

        private void FullClockCountdownMenuItem_Click(object sender, EventArgs e)
        {
            // Check if the FullClockCountdownBox is null or disposed, and create a new instance if necessary
            if (fullClockCountdownBox == null || fullClockCountdownBox.IsDisposed)
            {
                fullClockCountdownBox = new FullClockCountdownBox();
            }

            // Show the FullClockCountdownBox and start the countdown
            fullClockCountdownBox.Show();
            fullClockCountdownBox.StartCountdown();
        }

        private void ClockCountdownTimer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            int secondsToNextMark = GetSecondsToNextMark(now);

            if (secondsToNextMark <= 120) // 2 minutes or less remaining
            {
                clockCountdownTimer.Stop();
                remainingTime = secondsToNextMark; // Set remaining time for the countdown

                // Start the countdown
                StartCountdown();
            }
        }

        private int GetSecondsToNextMark(DateTime now)
        {
            // Calculate the time to the next 0-minute or 30-minute mark
            int minutes = now.Minute;
            int seconds = now.Second;

            if (minutes < 30)
            {
                // Next mark is the 30-minute mark
                return ((30 - minutes) * 60) - seconds;
            }
            else
            {
                // Next mark is the 0-minute mark of the next hour
                return ((60 - minutes) * 60) - seconds;
            }
        }

        private void StartCountdown()
        {
            var countdownTimer = new Timer
            {
                Interval = 1000 // 1 second
            };

            countdownTimer.Tick += (s, e) =>
            {
                remainingTime--;

                int minutes = remainingTime / 60;
                int seconds = remainingTime % 60;

                // Update the overlay form
                overlayForm.UpdateCountdown(minutes, seconds);

                if (remainingTime <= 0)
                {
                    countdownTimer.Stop();
                    overlayForm.Hide();
                    MessageBox.Show("Countdown finished!", "Info");
                }
            };

            // Set a random background color for the overlay form
            overlayForm.SetRandomBackgroundColor();

            // Show the overlay form
            overlayForm.Show();
            overlayForm.UpdateCountdown(remainingTime / 60, remainingTime % 60);

            countdownTimer.Start();
        }

        private void OpenExecutionForm(object sender, EventArgs e)
        {
            // Create and show an instance of ExecutionForm
            var executionForm = new ExecutionForm();
            executionForm.Show();
        }

        private void OpenAlertsForm(object sender, EventArgs e)
        {
            var alertsForm = new AlertsForm();
            alertsForm.Show();
        }

        private void BackgroundJobsMenuItem_Click(object sender, EventArgs e)
        {
            var backgroundJobsForm = new BackgroundJobsForm(backgroundJobs);
            backgroundJobsForm.ShowDialog();
        }

        private void BackgroundJobs_JobStatusUpdated(string message)
        {
            // Update the UI with the job status
            Invoke(new Action(() =>
            {
                statusTextBox.AppendText(message + Environment.NewLine);
            }));
        }
    }
}