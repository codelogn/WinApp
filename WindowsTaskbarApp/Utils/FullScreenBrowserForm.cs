using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

namespace WindowsTaskbarApp.Utils
{
    public class FullScreenBrowserForm : Form
    {
        private readonly string url;
        private readonly int refreshIntervalMinutes;
        private readonly WebView2 webView;
        private readonly Timer refreshTimer;
        private readonly Label countdownLabel;
        private readonly Button closeButton;
        private readonly Button fullScreenButton;
        private readonly Button refreshButton;
        private int remainingSeconds;
        private bool isFullScreen = true; // Start in full-screen mode by default

        public FullScreenBrowserForm(string url, int refreshIntervalMinutes)
        {
            this.url = url;
            this.refreshIntervalMinutes = refreshIntervalMinutes;
            this.remainingSeconds = refreshIntervalMinutes * 60;

            // Initialize WebView2
            this.webView = new WebView2();
            this.webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
            this.webView.NavigationStarting += WebView_NavigationStarting;
            this.webView.NavigationCompleted += WebView_NavigationCompleted;
            this.Controls.Add(webView);

            // Initialize Timer for Refresh
            this.refreshTimer = new Timer { Interval = 1000 }; // 1 second
            this.refreshTimer.Tick += RefreshTimer_Tick;
            this.refreshTimer.Start();

            // Initialize Countdown Label
            this.countdownLabel = new Label
            {
                Text = FormatTime(remainingSeconds),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(100, 0, 0, 0),
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true,
                Padding = new Padding(8),
                Visible = true
            };
            this.Controls.Add(countdownLabel);

            // Initialize Close Button
            this.closeButton = new Button
            {
                Text = "X",
                ForeColor = Color.White,
                BackColor = Color.Red,
                Font = new Font("Arial", 14, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(50, 50),
                Visible = true
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.FlatAppearance.MouseOverBackColor = Color.DarkRed;
            closeButton.Click += (sender, e) => this.Close();
            this.Controls.Add(closeButton);

            // Initialize Full-Screen Button
            this.fullScreenButton = new Button
            {
                Text = "Exit Full Screen",
                ForeColor = Color.White,
                BackColor = Color.Green,
                Font = new Font("Arial", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 40),
                Visible = true
            };
            fullScreenButton.FlatAppearance.BorderSize = 0;
            fullScreenButton.Click += ToggleFullScreen;
            this.Controls.Add(fullScreenButton);

            // Initialize Refresh Button
            this.refreshButton = new Button
            {
                Text = "Refresh",
                ForeColor = Color.White,
                BackColor = Color.Blue,
                Font = new Font("Arial", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 40),
                Visible = true
            };
            refreshButton.FlatAppearance.BorderSize = 0;
            refreshButton.Click += (sender, e) =>
            {
                this.webView.Reload();
                this.remainingSeconds = refreshIntervalMinutes * 60; // Reset the countdown
                countdownLabel.Text = FormatTime(remainingSeconds); // Update the countdown label
            };
            this.Controls.Add(refreshButton);

            // Load the URL
            InitializeWebView();

            // Arrange UI elements
            this.Resize += (sender, e) => ArrangeUIElements();
            ArrangeUIElements();

            // Start in full-screen mode
            this.Load += (sender, e) => ToggleFullScreen(null, null);
        }

        private void InitializeWebView()
        {
            this.webView.Source = new Uri(this.url);
        }

        private void WebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                MessageBox.Show("Failed to initialize WebView2.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void WebView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            // Optionally handle navigation starting events
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                MessageBox.Show("Failed to load the page.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            remainingSeconds--;

            if (remainingSeconds <= 0)
            {
                remainingSeconds = refreshIntervalMinutes * 60;
                this.webView.Reload();
            }

            countdownLabel.Text = FormatTime(remainingSeconds);
        }

        private string FormatTime(int seconds)
        {
            int minutes = seconds / 60;
            seconds %= 60;
            return $"{minutes:D2}:{seconds:D2}";
        }

        private void ToggleFullScreen(object sender, EventArgs e)
        {
            if (isFullScreen)
            {
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.fullScreenButton.Text = "Full Screen";
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
                this.FormBorderStyle = FormBorderStyle.None;
                this.fullScreenButton.Text = "Exit Full Screen";
            }

            isFullScreen = !isFullScreen;

            // Re-arrange UI elements after toggling full-screen mode
            ArrangeUIElements();
        }

        private void ArrangeUIElements()
        {
            // Resize the WebView2 control to leave space for the buttons and label
            webView.Location = new Point(0, 0);
            webView.Size = new Size(this.ClientSize.Width, this.ClientSize.Height);
            webView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Position the countdown label at the top-right corner
            countdownLabel.Location = new Point(this.ClientSize.Width - countdownLabel.Width - 10, 10);
            countdownLabel.BringToFront();

            // Position the close button at the top-left corner
            closeButton.Location = new Point(10, 10);
            closeButton.BringToFront();

            // Position the full-screen button below the close button
            fullScreenButton.Location = new Point(10, closeButton.Bottom + 10);
            fullScreenButton.BringToFront();

            // Position the refresh button below the full-screen button
            refreshButton.Location = new Point(10, fullScreenButton.Bottom + 10);
            refreshButton.BringToFront();
        }
    }
}