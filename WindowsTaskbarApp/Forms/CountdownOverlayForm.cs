using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowsTaskbarApp.Forms
{
    public partial class CountdownOverlayForm : Form
    {
        private Label countdownLabel;
        private Button stopButton;
        private bool isDragging = false;
        private Point dragStartPoint;

        public CountdownOverlayForm()
        {
            // Configure the form
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(300, 200); // Set an initial size for the form
            this.BackColor = Color.Black;
            this.Opacity = 0.8;

            // Configure the countdown label
            countdownLabel = new Label
            {
                ForeColor = Color.White,
                Font = new Font("Arial", 14, FontStyle.Bold),
                AutoSize = true, // Enable auto-sizing for the label
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top // Dock the label to the top
            };
            this.Controls.Add(countdownLabel);

            // Configure the stop button
            stopButton = new Button
            {
                Text = "Stop",
                Dock = DockStyle.Bottom,
                Height = 50
            };
            stopButton.Click += (s, e) => this.Hide();
            this.Controls.Add(stopButton);

            // Attach mouse events to the form and all child controls
            AttachMouseEvents(this);
        }

        private void AttachMouseEvents(Control control)
        {
            control.MouseDown += CountdownOverlayForm_MouseDown;
            control.MouseMove += CountdownOverlayForm_MouseMove;
            control.MouseUp += CountdownOverlayForm_MouseUp;

            // Recursively attach events to all child controls
            foreach (Control child in control.Controls)
            {
                AttachMouseEvents(child);
            }
        }

        public void SetRandomBackgroundColor()
        {
            Random random = new Random();
            this.BackColor = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
        }

        public void UpdateCountdown(int minutes, int seconds)
        {
            countdownLabel.Text = $"{minutes}m {seconds}s";
            AdjustFormSize(); // Adjust the form size based on the label content
        }

        public void UpdateCountdownOverlay((string label, int seconds)[] countdowns)
        {
            countdownLabel.Text = string.Join(Environment.NewLine, countdowns.Select(c =>
            {
                int minutes = c.seconds / 60;
                int seconds = c.seconds % 60;
                return $"{c.label}: {minutes}m {seconds}s";
            }));
            AdjustFormSize(); // Adjust the form size based on the label content
        }

        public void AddStopButton(Action onStop)
        {
            // Remove any previously attached event handlers to avoid duplication
            stopButton.Click -= (s, e) => this.Hide();
            stopButton.Click += (s, e) => onStop();
        }

        private void AdjustFormSize()
        {
            // Calculate the required size for the label
            countdownLabel.AutoSize = true;
            countdownLabel.PerformLayout();

            // Calculate the new width and height of the form
            int newWidth = Math.Max(countdownLabel.PreferredWidth + 20, stopButton.Width + 20); // Add padding
            int newHeight = countdownLabel.PreferredHeight + stopButton.Height + 40; // Add padding

            // Set the new size of the form
            this.Size = new Size(newWidth, newHeight);
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
                // Calculate the new position of the form
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