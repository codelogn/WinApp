using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsTaskbarApp.Forms
{
    public class ClockCountdownBox : BaseOverlayForm
    {
        private Label countdownLabel;
        private Timer countdownTimer;
        private bool isDragging = false;
        private Point dragStartPoint;

        public ClockCountdownBox()
        {
            // Set the size and background color
            this.Size = new Size(300, 150);
            this.BackColor = Color.LightGreen;

            // Configure the countdown label
            countdownLabel = new Label
            {
                Text = "Waiting for next mark...",
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(countdownLabel);

            // Initialize the timer
            countdownTimer = new Timer
            {
                Interval = 1000 // 1 second
            };
            countdownTimer.Tick += CountdownTimer_Tick;
        }

        public void StartCountdownToNextMark()
        {
            countdownTimer.Start();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            int secondsToNextMark = GetSecondsToNextMark(now);
            countdownLabel.Text = $"Next mark in {secondsToNextMark / 60}m {secondsToNextMark % 60}s";

            // Stop the timer if the countdown reaches zero
            if (secondsToNextMark <= 0)
            {
                countdownTimer.Stop();
                MessageBox.Show("Reached the next mark!", "Info");
                this.Hide();
            }
        }

        private int GetSecondsToNextMark(DateTime now)
        {
            int minutes = now.Minute;
            int seconds = now.Second;

            // Calculate seconds to the next 30-minute mark
            if (minutes < 30)
            {
                return ((30 - minutes) * 60) - seconds;
            }
            else
            {
                return ((60 - minutes) * 60) - seconds;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = e.Location;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isDragging)
            {
                this.Left += e.X - dragStartPoint.X;
                this.Top += e.Y - dragStartPoint.Y;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }
    }
}