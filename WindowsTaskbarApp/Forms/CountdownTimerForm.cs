using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsTaskbarApp.Forms
{
    public partial class CountdownTimerForm : Form
    {
        private NumericUpDown numericMinutes;
        private Timer countdownTimer;
        private NotifyIcon notifyIcon;
        private int remainingTime;
        private CountdownOverlayForm overlayForm;

        public CountdownTimerForm()
        {
            this.Text = "Countdown Timer";
            this.Size = new Size(300, 200);

            // Create MenuStrip
            var menuStrip = new MenuStrip();
            var toolsMenu = new ToolStripMenuItem("Tools");

            // Add "Start Timer" menu item
            var startTimerMenuItem = new ToolStripMenuItem("Start Timer");
            startTimerMenuItem.Click += StartTimerMenuItem_Click;
            toolsMenu.DropDownItems.Add(startTimerMenuItem);

            // Add Tools menu to the MenuStrip
            menuStrip.Items.Add(toolsMenu);
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            // Create numeric input for minutes
            var label = new Label
            {
                Text = "Minutes:",
                Location = new Point(20, 50),
                AutoSize = true
            };
            this.Controls.Add(label);

            numericMinutes = new NumericUpDown
            {
                Location = new Point(100, 50),
                Minimum = 1,
                Maximum = 1440, // Max 24 hours
                Value = 1
            };
            this.Controls.Add(numericMinutes);

            // Initialize Timer
            countdownTimer = new Timer
            {
                Interval = 1000 // 1 second
            };
            countdownTimer.Tick += CountdownTimer_Tick;

            // Initialize NotifyIcon
            notifyIcon = new NotifyIcon
            {
                Visible = false // Initially hidden
            };

            // Initialize the overlay form
            overlayForm = new CountdownOverlayForm();
        }

        private void StartTimerMenuItem_Click(object sender, EventArgs e)
        {
            remainingTime = (int)numericMinutes.Value * 60; // Convert minutes to seconds
            countdownTimer.Start();

            // Set a random background color for the overlay form
            overlayForm.SetRandomBackgroundColor();

            // Show the overlay form
            overlayForm.Show();
            overlayForm.UpdateCountdown(remainingTime / 60, remainingTime % 60);

            MessageBox.Show("Countdown started!", "Info");
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
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
        }
    }
}