using System;
using System.Drawing;
using System.Windows.Forms;

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

        public MainForm()
        {
            // Initialize the form
            this.Text = "Main Application";
            this.Size = new Size(400, 300);

            // Create a MenuStrip
            menuStrip = new MenuStrip();

            // Add "File" menu
            var fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("Open Form 1", null, OpenForm1);
            fileMenu.DropDownItems.Add("Text to Speech", null, OpenTextToSpeechForm);
            fileMenu.DropDownItems.Add("Exit", null, (s, e) => Application.Exit());
            menuStrip.Items.Add(fileMenu);

            // Add "Help" menu
            var helpMenu = new ToolStripMenuItem("Help");
            helpMenu.DropDownItems.Add("About", null, ShowAbout);
            menuStrip.Items.Add(helpMenu);

            // Add "Tools" menu
            var toolsMenu = new ToolStripMenuItem("Tools");
            toolsMenu.DropDownItems.Add("Countdown Timer", null, OpenCountdownTimerForm);

            // Add "Clock Countdown" menu item
            var clockCountdownMenuItem = new ToolStripMenuItem("Clock Countdown");
            clockCountdownMenuItem.Click += ClockCountdownMenuItem_Click;
            toolsMenu.DropDownItems.Add(clockCountdownMenuItem);

            menuStrip.Items.Add(toolsMenu);

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

        private void OpenForm1(object sender, EventArgs e)
        {
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
            var countdownTimerForm = new CountdownTimerForm();
            countdownTimerForm.Show();
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
            clockCountdownTimer.Start();
            MessageBox.Show("Clock Countdown started! Waiting for the next 2-minute mark.", "Info");
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
    }
}