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
        private bool isFlashing = false;
        private Timer flashTimer;
        private int flashColorIndex = 0; // Index to track the current color
        private readonly Color[] flashColors = { Color.Blue, Color.Yellow, Color.Red }; // Colors to cycle through

        public CountdownOverlayForm()
        {
            this.Text = "Countdown Overlay";
            this.Size = new Size((int)(150 * 0.65), (int)(80 * 0.65)); // Reduce size by 35%
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual; // Allow manual positioning
            this.TopMost = true;
            this.BackColor = Color.Black;
            this.Opacity = 0.8;

            // Set the initial position near the bottom-right corner of the screen
            var screen = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(screen.Width - this.Width - 20, screen.Height - this.Height - 20);

            // Initialize the flash timer
            flashTimer = new Timer
            {
                Interval = 200 // Change color every 200ms for a quick flashing effect
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
            // Generate a random color
            var random = new Random();
            this.BackColor = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw the countdown text
            using (var font = new Font("Arial", 24, FontStyle.Bold))
            {
                var graphics = e.Graphics;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                var textSize = graphics.MeasureString(countdownText, font);
                var textX = (this.Width - textSize.Width) / 2;
                var textY = (this.Height - textSize.Height) / 2;

                // Draw the text in white
                using (var brush = new SolidBrush(Color.White))
                {
                    graphics.DrawString(countdownText, font, brush, textX, textY);
                }
            }
        }

        private void FlashTimer_Tick(object sender, EventArgs e)
        {
            // Cycle through the colors
            this.BackColor = flashColors[flashColorIndex];
            flashColorIndex = (flashColorIndex + 1) % flashColors.Length; // Move to the next color
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