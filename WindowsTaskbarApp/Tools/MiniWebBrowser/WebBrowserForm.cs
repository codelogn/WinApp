using System;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace WindowsTaskbarApp.Tools.MiniWebBrowser
{
    public partial class WebBrowserForm : Form
    {
        private WebView2 webView;
        private TextBox urlTextBox;
        private Button loadButton;
        private ToolStrip browserToolStrip;

        public WebBrowserForm()
        {
            InitializeComponent();

            // Initialize WebView2 asynchronously in the Load event
            this.Load += WebBrowserForm_Load;
        }

        private async void WebBrowserForm_Load(object sender, EventArgs e)
        {
            try
            {
                await webView.EnsureCoreWebView2Async();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"WebView2 initialization failed: {ex.Message}");
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Web Browser";
            this.Size = new System.Drawing.Size(800, 600);

            // Initialize WebView2
            webView = new WebView2
            {
                Dock = DockStyle.Fill
            };

            // Ensure CoreWebView2 is initialized synchronously
            webView.CoreWebView2InitializationCompleted += (s, e) =>
            {
                // You can handle post-initialization logic here if needed
                // Set homepage to google.com on first load
                if (webView.CoreWebView2 != null)
                {
                    webView.CoreWebView2.Navigate("https://www.google.com");
                }
            };

            // Initialize URL TextBox
            urlTextBox = new TextBox
            {
                Dock = DockStyle.Top,
                PlaceholderText = "Enter URL here..."
            };
            // Make Enter key trigger LoadButton_Click
            urlTextBox.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    LoadButton_Click(sender, EventArgs.Empty);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };

            // Initialize Load Button
            loadButton = new Button
            {
                Text = "Load",
                Dock = DockStyle.Top
            };
            loadButton.Click += LoadButton_Click;

            // Initialize ToolStrip
            browserToolStrip = new ToolStrip
            {
                Dock = DockStyle.Top,
                GripStyle = ToolStripGripStyle.Hidden
            };

            // Add Back button
            var backButton = new ToolStripButton("Back")
            {
                Enabled = false // Initially disabled
            };
            backButton.Click += (sender, e) =>
            {
                if (webView.CanGoBack)
                {
                    webView.GoBack();
                }
            };
            browserToolStrip.Items.Add(backButton);

            // Add Forward button
            var forwardButton = new ToolStripButton("Forward")
            {
                Enabled = false // Initially disabled
            };
            forwardButton.Click += (sender, e) =>
            {
                if (webView.CanGoForward)
                {
                    webView.GoForward();
                }
            };
            browserToolStrip.Items.Add(forwardButton);

            // Add Refresh button
            var refreshButton = new ToolStripButton("Refresh");
            refreshButton.Click += (sender, e) => webView.Reload();
            browserToolStrip.Items.Add(refreshButton);

            // Add Stop button
            var stopButton = new ToolStripButton("Stop");
            stopButton.Click += (sender, e) => webView.CoreWebView2?.Stop();
            browserToolStrip.Items.Add(stopButton);

            // Add Home button
            var homeButton = new ToolStripButton("Home");
            homeButton.Click += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(urlTextBox.Text) && webView.CoreWebView2 != null)
                {
                    webView.CoreWebView2.Navigate(urlTextBox.Text);
                }
            };
            browserToolStrip.Items.Add(homeButton);

            // Add controls to the form
            this.Controls.Add(webView);
            this.Controls.Add(browserToolStrip);
            this.Controls.Add(loadButton);
            this.Controls.Add(urlTextBox);

            // Update button states based on navigation
            webView.NavigationStarting += (sender, e) =>
            {
                backButton.Enabled = webView.CanGoBack;
                forwardButton.Enabled = webView.CanGoForward;
            };
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(urlTextBox.Text) && webView.CoreWebView2 != null)
            {
                try
                {
                    string url = urlTextBox.Text.Trim();
                    if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        url = "https://" + url;
                    }
                    webView.CoreWebView2.Navigate(url);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load URL: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
