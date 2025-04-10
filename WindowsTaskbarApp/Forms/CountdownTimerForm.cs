using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace WindowsTaskbarApp.Forms
{
    public partial class CountdownTimerForm : Form
    {
        private NumericUpDown numericMinutes;
        private Button startButton;
        private Timer countdownTimer;
        private NotifyIcon notifyIcon;
        private int remainingTime;
        private CountdownOverlayForm overlayForm;

        public CountdownTimerForm()
        {
            this.Text = "Countdown Timer";
            this.Size = new Size(300, 150);

            // Create numeric input for minutes
            var label = new Label
            {
                Text = "Minutes:",
                Location = new Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(label);

            numericMinutes = new NumericUpDown
            {
                Location = new Point(100, 20),
                Minimum = 1,
                Maximum = 1440, // Max 24 hours
                Value = 1
            };
            this.Controls.Add(numericMinutes);

            // Create Start button
            startButton = new Button
            {
                Text = "Start",
                Location = new Point(100, 60),
                AutoSize = true
            };
            startButton.Click += StartButton_Click;
            this.Controls.Add(startButton);

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

        private void StartButton_Click(object sender, EventArgs e)
        {
            remainingTime = (int)numericMinutes.Value * 60; // Convert minutes to seconds
            countdownTimer.Start();
            startButton.Enabled = false;

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
                startButton.Enabled = true;

                // Hide the overlay form
                overlayForm.Hide();

                // Show Windows notification
                notifyIcon.BalloonTipTitle = "Countdown Timer";
                notifyIcon.BalloonTipText = "Time's up!";
                notifyIcon.ShowBalloonTip(3000);

                // Dispose NotifyIcon after showing the notification
                notifyIcon.BalloonTipClosed += (s, args) => notifyIcon.Dispose();
                notifyIcon.BalloonTipClicked += (s, args) => notifyIcon.Dispose();
            }
        }

        private void UpdateNotifyIcon(int minutes)
        {
            // Create a bitmap to display the remaining minutes
            using (var bitmap = new Bitmap(32, 32))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                graphics.Clear(Color.Transparent);

                // Draw the remaining minutes as text
                using (var font = new Font("Arial", 16, FontStyle.Bold, GraphicsUnit.Pixel))
                using (var brush = new SolidBrush(Color.Black))
                {
                    var text = minutes.ToString();
                    var textSize = graphics.MeasureString(text, font);
                    graphics.DrawString(text, font, brush, (bitmap.Width - textSize.Width) / 2, (bitmap.Height - textSize.Height) / 2);
                }

                // Update the NotifyIcon with the new bitmap
                notifyIcon.Icon = Icon.FromHandle(bitmap.GetHicon());
            }
        }
    }
}