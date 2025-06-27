using System;
using System.Drawing;
using System.Windows.Forms;
using WindowsTaskbarApp.Forms.Alerts;
using WindowsTaskbarApp.Jobs;
using WindowsTaskbarApp.Forms.Links;
using WindowsTaskbarApp.Tools.GroupLinks;
using WindowsTaskbarApp.Tools.MiniWebBrowser;
using System.Reflection;
using WindowsTaskbarApp.Tools;
using WindowsTaskbarApp.Tools.AudioPlayer;
using WindowsTaskbarApp.Forms.Jobs;
using WindowsTaskbarApp.Tools.TextToSpeech;
using WindowsTaskbarApp.Tools.Events;
using WindowsTaskbarApp.Forms.Logs;

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
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
            this.Text = $"Main Application v{version}";
            this.Size = new Size(400, 300);

            // Create a MenuStrip
            menuStrip = new MenuStrip();

            // Add "File" menu
            var fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("Exit", null, (s, e) => Application.Exit());
            menuStrip.Items.Add(fileMenu);

            // Add "Tools" menu
            var toolsMenu = new ToolStripMenuItem("Tools");
            toolsMenu.DropDownItems.Add("Open Web Browser - Custom", null, OpenWeb); // Moved from "File" to "Tools"

            // Add "Countdown Timer" menu item
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



            // Add "Open Web Browser" menu item
            var openWebBrowserMenuItem = new ToolStripMenuItem("Open Mini Web Browser");
            openWebBrowserMenuItem.Click += OpenWebBrowserForm;
            toolsMenu.DropDownItems.Add(openWebBrowserMenuItem);

            // Add "Test" menu under Tools
            var testMenu = new ToolStripMenuItem("Test");

            // Add "Test XML" submenu under Test
            var testXmlMenuItem = new ToolStripMenuItem("Test XML");
            testXmlMenuItem.Click += OpenTestXmlForm;
            testMenu.DropDownItems.Add(testXmlMenuItem);

            // Add "Text to Speech" submenu under Test
            var testTextToSpeechMenuItem = new ToolStripMenuItem("Text to Speech");
            testTextToSpeechMenuItem.Click += OpenTextToSpeechForm;
            testMenu.DropDownItems.Add(testTextToSpeechMenuItem);

            // Add "Test Audio" submenu under Test
            var testAudioMenuItem = new ToolStripMenuItem("Test Audio");
            testAudioMenuItem.Click += (s, e) =>
            {
                var audioPlayerForm = new AudioPlayerForm();
                audioPlayerForm.Show();
            };
            testMenu.DropDownItems.Add(testAudioMenuItem);

            // Add "Show Hello Notification" submenu under Test
            var helloNotificationMenuItem = new ToolStripMenuItem("Show Hello Notification");
            helloNotificationMenuItem.Click += ShowHelloNotification;
            testMenu.DropDownItems.Add(helloNotificationMenuItem);

            // Add Test menu to Tools menu
            toolsMenu.DropDownItems.Add(testMenu);

            // Add "Events" menu item under Tools
            var eventsMenuItem = new ToolStripMenuItem("Events");
            eventsMenuItem.Click += (s, e) =>
            {
                var eventsViewerForm = new EventsViewerForm();
                eventsViewerForm.Show();
            };
            toolsMenu.DropDownItems.Add(eventsMenuItem);

            menuStrip.Items.Add(toolsMenu);

            // Add "Admin" menu between "Tools" and "Help"
            var adminMenu = new ToolStripMenuItem("Admin");
            adminMenu.DropDownItems.Add("Manage Links", null, ShowLinksManagement); // Updated text and method

            // Move "Alerts" to Admin menu
            adminMenu.DropDownItems.Add("Manage Alerts", null, OpenAlertsForm);

            // Add Background Jobs menu item
            backgroundJobsMenuItem = new ToolStripMenuItem("Manage Jobs");
            backgroundJobsMenuItem.Click += BackgroundJobsMenuItem_Click;
            adminMenu.DropDownItems.Add(backgroundJobsMenuItem);

            // Add "Manage Configurations" submenu under Admin menu
            adminMenu.DropDownItems.Add("Manage Configurations", null, OpenManageConfigurationsForm);

            // Add "Manage Logs" submenu under Admin menu
            adminMenu.DropDownItems.Add("Manage Logs", null, (s, e) =>
            {
                var logsViewerForm = new LogsViewerForm();
                logsViewerForm.Show();
            });

            menuStrip.Items.Add(adminMenu);

            // Add "Help" menu after "Tools"
            var helpMenu = new ToolStripMenuItem("Help");
            helpMenu.DropDownItems.Add("About", null, ShowAbout);
            menuStrip.Items.Add(helpMenu);

            // Add the MenuStrip to the form
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);


            // Initialize Clock Countdown Timer
            clockCountdownTimer = new Timer
            {
                Interval = 1000 // Check every second
            };
            clockCountdownTimer.Tick += ClockCountdownTimer_Tick;

            // Initialize the overlay form
            overlayForm = new CountdownOverlayForm();
            this.Load += new System.EventHandler(this.MainForm_Load);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            backgroundJobs = new BackgroundJobs();
            backgroundJobs.JobStatusUpdated += BackgroundJobs_JobStatusUpdated;
        }

        private void OpenWeb(object sender, EventArgs e)
        {
            // Create and show an instance of Form1
            var form1 = new OpenWeb();
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

        private void ShowLinksManagement(object sender, EventArgs e)
        {
            var linksManagementForm = new LinksManagementForm();
            linksManagementForm.ShowDialog();
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
            if (backgroundJobs != null)
            {

                var backgroundJobsForm = new JobsForm(backgroundJobs);
                backgroundJobsForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Main form Background jobs instance is not initialized.");
            }

        }

        private void BackgroundJobs_JobStatusUpdated(string message)
        {
            // Update the UI with the job status
            Invoke(new Action(() =>
            {
                statusTextBox.AppendText(message + Environment.NewLine);
            }));
        }

        private void OpenWebBrowserForm(object sender, EventArgs e)
        {
            var webBrowserForm = new WebBrowserForm();
            webBrowserForm.Show();
        }

        private void OpenTestXmlForm(object sender, EventArgs e)
        {
            var testXmlForm = new TestXmlForm();
            testXmlForm.Show();
        }

        private void ShowHelloNotification(object sender, EventArgs e)
        {
            if (notifyIcon == null)
            {
                notifyIcon = new NotifyIcon
                {
                    Visible = true,
                    Icon = SystemIcons.Information,
                    BalloonTipTitle = "Notification",
                    BalloonTipText = "Hello World"
                };
                notifyIcon.BalloonTipClicked += NotifyIcon_BalloonTipClicked;
            }
            notifyIcon.BalloonTipTitle = "Notification";
            notifyIcon.BalloonTipText = "Hello World";
            notifyIcon.ShowBalloonTip(3000);
        }

        private void NotifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            // Open a URL in the default browser
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://www.google.com",
                UseShellExecute = true
            });
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LinksEvents.LinksChanged += (s, args) => RefreshGroupLinksMenu();
        }

        private void RefreshGroupLinksMenu()
        {
            try
            {
                GroupLinksTrayIcon groupLinksTrayIcon = GroupLinksTrayIcon.Instance;
                groupLinksTrayIcon.RefreshGroupLinksMenu();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing group links menu: {ex.Message}", "Error");
            }
        }

        private void OpenManageConfigurationsForm(object sender, EventArgs e)
        {
            var configForm = new WindowsTaskbarApp.Forms.Configurations.ManageConfigurationForm();
            configForm.ShowDialog();
        }
    }
}