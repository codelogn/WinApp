using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsTaskbarApp.Forms
{
    public partial class CountdownOverlayForm : Form
    {
        private string countdownText = "00:00";
        private bool isDragging = false;
        private Point dragStartPoint;
        private Timer flashTimer;
        private bool isFlashing = false;
        private Random random;

        public CountdownOverlayForm()
        {
            this.Text = "Countdown Overlay";
            this.Size = new Size(150, 80); // Slightly smaller size
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual; // Allow manual positioning
            this.TopMost = true;
            this.BackColor = Color.Black;
            this.Opacity = 0.5;

            // Initialize random generator
            random = new Random();

            // Set the initial position near the bottom-right corner of the screen
            var screen = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(screen.Width - this.Width - 20, screen.Height - this.Height - 20);

            // Initialize the flash timer
            flashTimer = new Timer
            {
                Interval = 500 // Flash every 500ms (half a second)
            };
            flashTimer.Tick += FlashTimer_Tick;

            // Enable mouse events for dragging
            this.MouseDown += CountdownOverlayForm_MouseDown;
            this.MouseMove += CountdownOverlayForm_MouseMove;
            this.MouseUp += CountdownOverlayForm_MouseUp;
        }

        public void UpdateCountdown(int minutes, int seconds)
        {
            countdownText = $"{minutes:D2}:{seconds:D2}";
            this.Invalidate(); // Force repaint if needed

            // Start flashing effect when less than 1 minute remains
            if (minutes == 0 && !isFlashing)
            {
                isFlashing = true;
                flashTimer.Start();
            }
            else if (minutes > 0 && isFlashing)
            {
                isFlashing = false;
                flashTimer.Stop();
                this.BackColor = Color.Black; // Reset background color
            }
        }

        public void SetRandomBackgroundColor()
        {
            // Generate random RGB values
            int r = random.Next(0, 256);
            int g = random.Next(0, 256);
            int b = random.Next(0, 256);

            // Set the background color
            this.BackColor = Color.FromArgb(r, g, b);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw the countdown text with a black border
            using (var font = new Font("Arial", 24, FontStyle.Bold))
            {
                var graphics = e.Graphics;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                var textSize = graphics.MeasureString(countdownText, font);
                var textX = (this.Width - textSize.Width) / 2;
                var textY = (this.Height - textSize.Height) / 2;

                // Draw the border by drawing the text multiple times with slight offsets
                using (var brush = new SolidBrush(Color.Black))
                {
                    graphics.DrawString(countdownText, font, brush, textX - 1, textY - 1);
                    graphics.DrawString(countdownText, font, brush, textX + 1, textY - 1);
                    graphics.DrawString(countdownText, font, brush, textX - 1, textY + 1);
                    graphics.DrawString(countdownText, font, brush, textX + 1, textY + 1);
                }

                // Draw the main text in white
                using (var brush = new SolidBrush(Color.White))
                {
                    graphics.DrawString(countdownText, font, brush, textX, textY);
                }
            }
        }

        private void FlashTimer_Tick(object sender, EventArgs e)
        {
            // Toggle the background color between black and red
            this.BackColor = this.BackColor == Color.Black ? Color.Red : Color.Black;
        }

        private void CountdownOverlayForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = e.Location;
            }
        }

        private void CountdownOverlayForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                this.Left += e.X - dragStartPoint.X;
                this.Top += e.Y - dragStartPoint.Y;
            }
        }

        private void CountdownOverlayForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }
    }
}