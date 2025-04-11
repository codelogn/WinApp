using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsTaskbarApp.Forms
{
    public class CountdownTimerBox : Form
    {
        private Timer countdownTimer;
        private Timer flashyBackgroundTimer; // Timer for flashy background effect
        private bool isFlashing = false; // Indicates if the flashy effect is active
        private bool isRed = true; // Tracks the current color during flashing
        private bool isDragging = false;
        private Point dragStartPoint;
        private int remainingTime;

        public CountdownTimerBox()
        {
            // Set the size and background color
            this.Size = new Size(100, 60); // Smallest possible size
            this.FormBorderStyle = FormBorderStyle.None; // Remove the title bar
            this.TopMost = true; // Ensure the form is always on top
            this.Opacity = 0.8; // Make the box more transparent

            // Enable double buffering for smoother rendering
            this.DoubleBuffered = true;

            // Initialize the countdown timer
            countdownTimer = new Timer
            {
                Interval = 1000 // 1 second
            };
            countdownTimer.Tick += CountdownTimer_Tick;

            // Initialize the flashy background timer
            flashyBackgroundTimer = new Timer
            {
                Interval = 200 // Flash every 200ms
            };
            flashyBackgroundTimer.Tick += FlashyBackgroundTimer_Tick;

            // Attach mouse events for dragging
            AttachMouseEvents(this);

            // Attach mouse click event to change background color
            this.MouseClick += CountdownTimerBox_MouseClick;
        }

        public void StartCountdown(int minutes, int seconds)
        {
            remainingTime = (minutes * 60) + seconds;
            SetRandomBackgroundColor();
            countdownTimer.Start();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (remainingTime > 0)
            {
                remainingTime--;

                // Start flashy background effect when 1 minute is left
                if (remainingTime == 60 && !isFlashing)
                {
                    isFlashing = true;
                    flashyBackgroundTimer.Start();
                }

                this.Invalidate(); // Redraw the form to update the countdown
            }
            else
            {
                countdownTimer.Stop();
                flashyBackgroundTimer.Stop(); // Stop the flashy effect when countdown ends
                MessageBox.Show("Countdown finished!", "Info");
                this.Close();
            }
        }

        private void FlashyBackgroundTimer_Tick(object sender, EventArgs e)
        {
            // Alternate between red and blue colors
            this.BackColor = isRed ? Color.Red : Color.Blue;
            isRed = !isRed;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Calculate minutes and seconds remaining
            int minutes = remainingTime / 60;
            int seconds = remainingTime % 60;

            string countdownText = $"{minutes}m {seconds}s";

            // Draw the countdown text centered
            var graphics = e.Graphics;
            var font = new Font("Arial", 12, FontStyle.Bold);
            var textBrush = new SolidBrush(Color.White);
            var borderBrush = new SolidBrush(Color.Black);

            var textSize = graphics.MeasureString(countdownText, font);
            var x = (this.ClientSize.Width - textSize.Width) / 2; // Center horizontally
            var y = (this.ClientSize.Height - textSize.Height) / 2; // Center vertically

            // Draw border (shadow effect)
            graphics.DrawString(countdownText, font, borderBrush, x - 1, y - 1);
            graphics.DrawString(countdownText, font, borderBrush, x + 1, y - 1);
            graphics.DrawString(countdownText, font, borderBrush, x - 1, y + 1);
            graphics.DrawString(countdownText, font, borderBrush, x + 1, y + 1);

            // Draw the main text
            graphics.DrawString(countdownText, font, textBrush, x, y);
        }

        private void SetRandomBackgroundColor()
        {
            var random = new Random();
            this.BackColor = Color.FromArgb(random.Next(100, 256), random.Next(100, 256), random.Next(100, 256));
        }

        private void CountdownTimerBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                SetRandomBackgroundColor(); // Change background color on left click
            }
        }

        private void AttachMouseEvents(Control control)
        {
            control.MouseDown += CountdownTimerBox_MouseDown;
            control.MouseMove += CountdownTimerBox_MouseMove;
            control.MouseUp += CountdownTimerBox_MouseUp;

            // Recursively attach events to all child controls
            foreach (Control child in control.Controls)
            {
                AttachMouseEvents(child);
            }
        }

        private void CountdownTimerBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = e.Location;
            }
        }

        private void CountdownTimerBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                this.Left += e.X - dragStartPoint.X;
                this.Top += e.Y - dragStartPoint.Y;
            }
        }

        private void CountdownTimerBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }
    }
}