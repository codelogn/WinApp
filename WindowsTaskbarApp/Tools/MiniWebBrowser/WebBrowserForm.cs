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
                // Set default URL
                urlTextBox.Text = "https://www.google.com";
                if (webView.CoreWebView2 != null)
                {
                    webView.CoreWebView2.Navigate(urlTextBox.Text);
                }
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

            // Initialize URL TextBox
            urlTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = "Enter URL here..."
            };
            urlTextBox.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    LoadButton_Click(sender, EventArgs.Empty);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };

            // Initialize Load Button (small, next to URL box)
            loadButton = new Button
            {
                Text = "Load",
                Width = 60,
                Dock = DockStyle.Fill,
                Margin = new Padding(2, 0, 2, 0)
            };
            loadButton.Click += LoadButton_Click;

            // TableLayoutPanel for URL and Load button
            var urlPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 28,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(2),
                AutoSize = true
            };
            urlPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            urlPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
            urlPanel.Controls.Add(urlTextBox, 0, 0);
            urlPanel.Controls.Add(loadButton, 1, 0);

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
                LoadButton_Click(sender, e);
            };
            browserToolStrip.Items.Add(homeButton);

            // Add controls to the form
            this.Controls.Add(webView);
            this.Controls.Add(browserToolStrip);
            this.Controls.Add(urlPanel); // Add urlPanel instead of urlTextBox and loadButton separately

            // Update button states based on navigation
            webView.NavigationStarting += (sender, e) =>
            {
                backButton.Enabled = webView.CanGoBack;
                forwardButton.Enabled = webView.CanGoForward;
            };
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            if (webView.CoreWebView2 == null)
                return;

            string url = urlTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(url))
                return;

            // Prepend https:// if missing
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }

            // Validate URL
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) &&
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                try
                {
                    webView.CoreWebView2.Navigate(url);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load URL: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid URL.", "Invalid URL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
