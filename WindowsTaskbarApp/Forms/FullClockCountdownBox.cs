using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowsTaskbarApp.Forms
{
    public class FullClockCountdownBox : Form
    {
        private Timer countdownTimer;
        private bool isDragging = false;
        private Point dragStartPoint;
        private Button exitButton;

        public FullClockCountdownBox()
        {
            // Set the size and background color
            this.Size = new Size(200, 150); // Reduced width but kept height the same
            this.FormBorderStyle = FormBorderStyle.None; // Remove the title bar
            this.TopMost = true; // Ensure the form is always on top
            this.Opacity = 0.8; // Make the box more transparent

            // Enable double buffering for smoother rendering
            this.DoubleBuffered = true;

            // Add an Exit button
            exitButton = new Button
            {
                Text = "Exit",
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = Color.Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            exitButton.Click += (s, e) => this.Close();
            this.Controls.Add(exitButton);

            // Initialize the timer
            countdownTimer = new Timer
            {
                Interval = 1000 // 1 second
            };
            countdownTimer.Tick += CountdownTimer_Tick;

            // Attach mouse events for dragging
            AttachMouseEvents(this);

            // Attach mouse click event to change background color
            this.MouseClick += FullClockCountdownBox_MouseClick;
        }

        public void StartCountdown()
        {
            SetRandomBackgroundColor();
            countdownTimer.Start();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            // Redraw the form to update the countdown
            this.Invalidate(); // Triggers the Paint event
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var now = DateTime.Now;

            // Calculate remaining time for each interval
            int secondsToNext1Min = 60 - now.Second;
            int secondsToNext5Min = (5 - (now.Minute % 5)) * 60 - now.Second;
            int secondsToNext15Min = (15 - (now.Minute % 15)) * 60 - now.Second;
            int secondsToNext30Min = (30 - (now.Minute % 30)) * 60 - now.Second;
            int secondsToNextHour = (60 - now.Minute) * 60 - now.Second;

            string[] countdowns = new[]
            {
                $"1M: {secondsToNext1Min / 60}m {secondsToNext1Min % 60}s",
                $"5M: {secondsToNext5Min / 60}m {secondsToNext5Min % 60}s",
                $"15M: {secondsToNext15Min / 60}m {secondsToNext15Min % 60}s",
                $"30M: {secondsToNext30Min / 60}m {secondsToNext30Min % 60}s",
                $"1H: {secondsToNextHour / 60}m {secondsToNextHour % 60}s"
            };

            // Draw the countdown text centered
            var graphics = e.Graphics;
            var font = new Font("Arial", 10, FontStyle.Bold);
            var textBrush = new SolidBrush(Color.White);
            var borderBrush = new SolidBrush(Color.Black);

            var totalHeight = countdowns.Length * 20; // Calculate total height of all lines
            var startY = (this.ClientSize.Height - totalHeight) / 2 - 10; // Center vertically and move slightly up

            for (int i = 0; i < countdowns.Length; i++)
            {
                var text = countdowns[i];
                var textSize = graphics.MeasureString(text, font);
                var x = (this.ClientSize.Width - textSize.Width) / 2; // Center horizontally
                var y = startY + (i * 20);

                // Draw border (shadow effect)
                graphics.DrawString(text, font, borderBrush, x - 1, y - 1);
                graphics.DrawString(text, font, borderBrush, x + 1, y - 1);
                graphics.DrawString(text, font, borderBrush, x - 1, y + 1);
                graphics.DrawString(text, font, borderBrush, x + 1, y + 1);

                // Draw the main text
                graphics.DrawString(text, font, textBrush, x, y);
            }
        }

        private void SetRandomBackgroundColor()
        {
            var random = new Random();
            this.BackColor = Color.FromArgb(random.Next(100, 256), random.Next(100, 256), random.Next(100, 256));
        }

        private void FullClockCountdownBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                SetRandomBackgroundColor(); // Change background color on left click
            }
        }

        private void AttachMouseEvents(Control control)
        {
            control.MouseDown += FullClockCountdownBox_MouseDown;
            control.MouseMove += FullClockCountdownBox_MouseMove;
            control.MouseUp += FullClockCountdownBox_MouseUp;

            // Recursively attach events to all child controls
            foreach (Control child in control.Controls)
            {
                AttachMouseEvents(child);
            }
        }

        private void FullClockCountdownBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = e.Location;
            }
        }

        private void FullClockCountdownBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                this.Left += e.X - dragStartPoint.X;
                this.Top += e.Y - dragStartPoint.Y;
            }
        }

        private void FullClockCountdownBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }
    }
}