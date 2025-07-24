using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WindowsTaskbarApp.Tools.MiniWebBrowser
{
    public partial class WebBrowserForm : Form
    {
        private WebView2 webView;
        private TextBox urlTextBox;
        private Button loadButton;
        private ToolStrip browserToolStrip;

        private bool isFileWriteActive = false;
        private string lastDomContent = "";
        private string lastWrittenDomContent = "";
        private string currentUrlFilePath = null;
        private System.Timers.Timer domCheckTimer;
        private ToolStripButton recButton;
        private System.Windows.Forms.Timer recBlinkTimer;
        private bool recBlinkState = false;

        public WebBrowserForm()
        {
            InitializeComponent();
            this.Load += WebBrowserForm_Load;
            // Start recording by default
            recButton?.PerformClick();
        }

        private async void WebBrowserForm_Load(object sender, EventArgs e)
        {
            try
            {
                await webView.EnsureCoreWebView2Async();
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

            webView = new WebView2
            {
                Dock = DockStyle.Fill
            };

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

            loadButton = new Button
            {
                Text = "Load",
                Width = 60,
                Dock = DockStyle.Fill,
                Margin = new Padding(2, 0, 2, 0)
            };
            loadButton.Click += LoadButton_Click;

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

            browserToolStrip = new ToolStrip
            {
                Dock = DockStyle.Top,
                GripStyle = ToolStripGripStyle.Hidden
            };

            var backButton = new ToolStripButton("Back")
            {
                Enabled = false
            };
            backButton.Click += (sender, e) =>
            {
                if (webView.CanGoBack)
                {
                    webView.GoBack();
                }
            };
            browserToolStrip.Items.Add(backButton);

            var forwardButton = new ToolStripButton("Forward")
            {
                Enabled = false
            };
            forwardButton.Click += (sender, e) =>
            {
                if (webView.CanGoForward)
                {
                    webView.GoForward();
                }
            };
            browserToolStrip.Items.Add(forwardButton);

            var refreshButton = new ToolStripButton("Refresh");
            refreshButton.Click += (sender, e) => webView.Reload();
            browserToolStrip.Items.Add(refreshButton);

            var stopButton = new ToolStripButton("Stop");
            stopButton.Click += (sender, e) => webView.CoreWebView2?.Stop();
            browserToolStrip.Items.Add(stopButton);

            var homeButton = new ToolStripButton("Home");
            homeButton.Click += (sender, e) =>
            {
                LoadButton_Click(sender, e);
            };
            browserToolStrip.Items.Add(homeButton);

            // Add REC button
            recButton = new ToolStripButton("● REC");
            recButton.ForeColor = System.Drawing.Color.Red;
            recButton.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
            recButton.ToolTipText = "Toggle recording";
            recButton.Click += (sender, e) =>
            {
                if (isFileWriteActive)
                {
                    // Stop recording
                    isFileWriteActive = false;
                    domCheckTimer?.Stop();
                    domCheckTimer?.Dispose();
                    domCheckTimer = null;
                    recButton.ForeColor = System.Drawing.Color.Gray;
                    recBlinkTimer?.Stop();
                    recButton.Text = "● REC";
                }
                else
                {
                    // Start recording
                    string dateStr = DateTime.Now.ToString("MM-dd-yyyy");
                    string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WinAppWebContent", dateStr);
                    Directory.CreateDirectory(folder);
                    isFileWriteActive = true;
                    lastDomContent = "";
                    lastWrittenDomContent = "";
                    currentUrlFilePath = null;
                    domCheckTimer = new System.Timers.Timer(1000); // 1 second
                    domCheckTimer.Elapsed += async (s2, args2) =>
                    {
                        if (isFileWriteActive && webView.CoreWebView2 != null)
                        {
                            try
                            {
                                string js = @"
    (() => {
        function isVisible(el) {
            return !!(el.offsetWidth || el.offsetHeight || el.getClientRects().length) && getComputedStyle(el).visibility !== 'hidden' && getComputedStyle(el).display !== 'none';
        }
        function getTextContent() {
            let result = '';
            let title = document.title || '';
            if (title) result += 'TITLE: ' + title + '\n';
            let desc = '';
            let metaDesc = document.querySelector('meta[name=description]');
            if (metaDesc && metaDesc.content) desc = metaDesc.content.trim();
            if (desc) result += 'DESCRIPTION: ' + desc + '\n';
            let main = document.querySelector('[role=main], main, article, section');
            if (main && isVisible(main)) {
                result += 'MAIN: ' + main.innerText.trim() + '\n';
            }
            document.querySelectorAll('h1,h2,h3,h4,h5,h6').forEach(h => {
                if (isVisible(h)) result += h.tagName + ': ' + h.innerText.trim() + '\n';
            });
            document.querySelectorAll('p').forEach(p => {
                if (isVisible(p)) result += 'P: ' + p.innerText.trim() + '\n';
            });
            document.querySelectorAll('div,span').forEach(el => {
                if (isVisible(el)) {
                    let role = el.getAttribute('role') || '';
                    let id = el.id || '';
                    let cls = el.className || '';
                    if (!/nav|footer|sidebar|menu|ads|ad|code|script|style/i.test(role+id+cls)) {
                        let txt = el.innerText.trim();
                        if (txt.length > 40) result += 'GENERALTEXT: ' + txt + '\n';
                    }
                }
            });
            document.querySelectorAll('a[href]').forEach(a => {
                if (isVisible(a)) result += 'LINK: ' + a.innerText.trim() + ' -> ' + a.href + '\n';
            });
            return result;
        }
        return getTextContent();
    })();
";
                                var currentDomContent = await webView.CoreWebView2.ExecuteScriptAsync(js);
                                currentDomContent = System.Text.Json.JsonSerializer.Deserialize<string>(currentDomContent);
                                var url = webView.Source?.AbsoluteUri ?? urlTextBox.Text.Trim();
                                var safeFileName = GetSafeFileName(url);
                                var dateFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WinAppWebContent", DateTime.Now.ToString("MM-dd-yyyy"));
                                var filePath = Path.Combine(dateFolder, safeFileName + ".yaml");
                                currentUrlFilePath = filePath;
                                if (currentDomContent != lastWrittenDomContent)
                                {
                                var yamlSerializer = new SerializerBuilder()
                                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                    .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
                                    .Build();
                                var yamlObj = new WebContentYaml
                                {
                                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                    Url = url,
                                    Content = currentDomContent
                                };
                                var yaml = yamlSerializer.Serialize(yamlObj);
                                File.WriteAllText(filePath, yaml);
                                lastWrittenDomContent = currentDomContent;
                                }
                            }
                            catch { }
                        }
                    };
                    domCheckTimer.Start();
                    recButton.ForeColor = System.Drawing.Color.Red;
                    recButton.Text = "● REC";
                    // Blinking effect
                    recBlinkTimer = new System.Windows.Forms.Timer();
                    recBlinkTimer.Interval = 500;
                    recBlinkTimer.Tick += (s3, e3) =>
                    {
                        recBlinkState = !recBlinkState;
                        recButton.ForeColor = recBlinkState ? System.Drawing.Color.Red : System.Drawing.Color.DarkRed;
                    };
                    recBlinkTimer.Start();
                }
            };
            browserToolStrip.Items.Add(recButton);

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true
            };
            mainPanel.ColumnCount = 1;
            mainPanel.RowCount = 2;
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainPanel.Controls.Add(urlPanel, 0, 0);
            mainPanel.Controls.Add(webView, 0, 1);

            this.Controls.Add(mainPanel);
            this.Controls.Add(browserToolStrip);
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            var url = urlTextBox.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("Please enter a valid URL.", "Invalid URL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Ensure the URL is well-formed and absolute
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                // Try to prepend https:// if missing
                url = "https://" + url;
                if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                {
                    MessageBox.Show("Please enter a valid absolute URL (e.g. https://example.com)", "Invalid URL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            try
            {
                webView.CoreWebView2.Navigate(uri.AbsoluteUri);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load URL: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetSafeFileName(string url)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var safe = new string(url.ToCharArray().Where(c => !invalidChars.Contains(c)).ToArray());
            if (safe.Length > 100) safe = safe.Substring(0, 100);
            return safe;
        }
        // Helper class for YAML serialization with proper indentation
        public class WebContentYaml
        {
            public string Timestamp { get; set; }
            public string Url { get; set; }
            public string Content { get; set; }
        }
    }
}
