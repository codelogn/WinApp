using System;
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
        private Panel browserContentPanel;
        private TabControl tabControl;
        private ToolStrip browserToolStrip;
        private TextBox addressTextBox;

        private bool isFileWriteActive = false;
        private string lastDomContent = "";
        private string lastWrittenDomContent = "";
        private string currentUrlFilePath = null;
        private System.Timers.Timer domCheckTimer;
        private ToolStripButton recButton;

        private void LoadTabUrl(WebView2 webView, TextBox urlTextBox)
        {
            var url = urlTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(url))
            {
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "https://" + url;
                }
                webView.CoreWebView2?.Navigate(url);
            }
        }
        private System.Windows.Forms.Timer recBlinkTimer;
        private bool recBlinkState = false;

        public WebBrowserForm()
        {
            InitializeComponent();
            this.Load += WebBrowserForm_Load;
        }

        private async void WebBrowserForm_Load(object sender, EventArgs e)
        {
            // Add first tab on load
            AddNewTab("https://www.google.com");
        }

        private void InitializeComponent()
        {
            this.Text = "Web Browser";
            this.Size = new System.Drawing.Size(1000, 700);

            // Main vertical layout
            var mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.RowCount = 3;
            mainLayout.ColumnCount = 1;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F)); // Top bar (slightly larger)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F)); // Address bar (slightly larger)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Browser content


            // Top bar: tabs and plus button (side by side), REC button separate
            var topBar = new TableLayoutPanel();
            topBar.Dock = DockStyle.Fill;
            topBar.Height = 40;
            topBar.ColumnCount = 2;
            topBar.RowCount = 1;
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Tabs + plus
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F)); // REC

            var tabsPanel = new FlowLayoutPanel();
            tabsPanel.Dock = DockStyle.Fill;
            tabsPanel.Height = 40;
            tabsPanel.FlowDirection = FlowDirection.LeftToRight;
            tabsPanel.WrapContents = false;

            tabControl = new TabControl
            {
                Dock = DockStyle.Left,
                Height = 40,
                Width = 800
            };
            tabControl.SelectedIndexChanged += (s, e) => {
                ShowActiveTabBrowser();
                // Sync address bar with active tab's URL
                if (tabControl.SelectedTab != null)
                {
                    var webView = tabControl.SelectedTab.Tag as WebView2;
                    if (webView != null && webView.CoreWebView2 != null)
                    {
                        addressTextBox.Text = webView.Source?.ToString() ?? "";
                    }
                }
            };
            tabControl.MouseDoubleClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    if (tabControl.TabCount > 1)
                        tabControl.TabPages.Remove(tabControl.SelectedTab);
                }
            };

            var plusTabButton = new Button();
            plusTabButton.Text = "+";
            plusTabButton.Font = new System.Drawing.Font("Arial", 12);
            plusTabButton.Size = new System.Drawing.Size(32, 32);
            plusTabButton.Cursor = Cursors.Hand;
            plusTabButton.FlatStyle = FlatStyle.Flat;
            plusTabButton.TabStop = false;
            plusTabButton.Margin = new Padding(0, 0, 0, 0);
            plusTabButton.Click += (sender, e) => AddNewTab("https://www.google.com");

            tabsPanel.Controls.Add(tabControl);
            tabsPanel.Controls.Add(plusTabButton);

            // REC button logic
            topBar.Controls.Add(tabsPanel, 0, 0);

            // Address bar
            var addressBarPanel = new TableLayoutPanel();
            addressBarPanel.Dock = DockStyle.Top;
            addressBarPanel.ColumnCount = 3;
            addressBarPanel.RowCount = 1;
            addressBarPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F)); // Address bar height
            addressBarPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            addressBarPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
            addressBarPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));

            addressTextBox = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Enter URL here..." };
            addressTextBox.Font = new System.Drawing.Font("Roboto", 10, System.Drawing.FontStyle.Regular);
            var goButton = new Button {
                Text = "Go",
                Dock = DockStyle.Fill,
                Width = 40,
                Height = 30,
                MinimumSize = new System.Drawing.Size(40, 32),
                Padding = new Padding(0),
                Margin = new Padding(0, 1, 0, 1),
                FlatStyle = FlatStyle.Standard
            };

            // Go button click navigates active tab's WebView2
            goButton.Click += (sender, e) => {
                if (tabControl.SelectedTab != null)
                {
                    var webView = tabControl.SelectedTab.Tag as WebView2;
                    if (webView != null && webView.CoreWebView2 != null)
                    {
                        var url = addressTextBox.Text.Trim();
                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                                url = "https://" + url;
                            lastWrittenDomContent = ""; // Reset so new content is written
                            webView.CoreWebView2.Navigate(url);
                        }
                    }
                }
            };

            // Enter key in address bar navigates active tab's WebView2
            addressTextBox.KeyDown += (sender, e) => {
                if (e.KeyCode == Keys.Enter)
                {
                    if (tabControl.SelectedTab != null)
                    {
                        var webView = tabControl.SelectedTab.Tag as WebView2;
                        if (webView != null && webView.CoreWebView2 != null)
                        {
                            var url = addressTextBox.Text.Trim();
                            if (!string.IsNullOrWhiteSpace(url))
                            {
                                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                                    url = "https://" + url;
                                lastWrittenDomContent = ""; // Reset so new content is written
                                webView.CoreWebView2.Navigate(url);
                            }
                        }
                    }
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };
            // REC button for address bar (use Button, not ToolStripButton)
            var recBtn = new Button {
                Text = "● REC",
                Dock = DockStyle.Fill,
                Width = 80,
                Height = 30,
                MinimumSize = new System.Drawing.Size(60, 32),
                Padding = new Padding(0),
                Margin = new Padding(0, 1, 0, 1),
                ForeColor = System.Drawing.Color.Gray,
                Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                FlatStyle = FlatStyle.Standard
            };
            recBtn.Click += (sender, e) => {
                recBlinkState = !recBlinkState;
                isFileWriteActive = recBlinkState;
                if (recBlinkState)
                {
                    recBtn.Text = "● REC";
                    recBtn.ForeColor = System.Drawing.Color.Red;
                    recBlinkTimer = new System.Windows.Forms.Timer();
                    recBlinkTimer.Interval = 500;
                    bool isRed = true;
                    recBlinkTimer.Tick += (s2, e2) => {
                        isRed = !isRed;
                        recBtn.ForeColor = isRed ? System.Drawing.Color.Red : System.Drawing.Color.Gray;
                    };
                    recBlinkTimer.Start();
                    StartContentCapture();
                }
                else
                {
                    if (recBlinkTimer != null)
                    {
                        recBlinkTimer.Stop();
                        recBlinkTimer.Dispose();
                        recBlinkTimer = null;
                    }
                    recBtn.ForeColor = System.Drawing.Color.Gray;
                    recBtn.Text = "● REC";
                    StopContentCapture();
                }
            };
            addressBarPanel.Controls.Add(addressTextBox, 0, 0);
            addressBarPanel.Controls.Add(goButton, 1, 0);
            addressBarPanel.Controls.Add(recBtn, 2, 0);

            // Browser content panel
            browserContentPanel = new Panel { Dock = DockStyle.Fill };

            // Add to main layout
            mainLayout.Controls.Add(topBar, 0, 0);
            mainLayout.Controls.Add(addressBarPanel, 0, 1);
            mainLayout.Controls.Add(browserContentPanel, 0, 2);

            this.Controls.Clear();
            this.Controls.Add(mainLayout);
        }

        private void AddNewTab(string initialUrl)
        {
            var tabPage = new TabPage();
            // Add close button to tab header
            tabPage.Text = "New Tab  ✕";
            tabPage.MouseDown += (sender, e) => {
                // Detect click on close button area (rightmost 24px)
                if (e.Button == MouseButtons.Left && e.X > tabPage.Width - 24)
                {
                    if (tabControl.TabPages.Count > 1)
                        tabControl.TabPages.Remove(tabPage);
                }
            };
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0),
                AutoSize = true
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var urlPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0),
                AutoSize = true,
                Height = 40
            };
            urlPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            urlPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));

            var urlTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = "Enter URL here..."
            };
            var loadButton = new Button
            {
                Text = "Load",
                Dock = DockStyle.Fill,
                Width = 60
            };
            var webView = new WebView2
            {
                Dock = DockStyle.Fill
            };

            tabPage.Tag = webView; // Store reference to WebView2 in tab's Tag
            // Sync address bar with navigation in this tab
            webView.NavigationStarting += (s, e) => {
                addressTextBox.Invoke((MethodInvoker)(() => addressTextBox.Text = e.Uri));
            };
            webView.NavigationCompleted += (s, e) => {
                addressTextBox.Invoke((MethodInvoker)(() => addressTextBox.Text = webView.Source?.ToString() ?? ""));
            };

            urlTextBox.Text = initialUrl;
            urlTextBox.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    lastWrittenDomContent = ""; // Reset so new content is written
                    LoadTabUrl(webView, urlTextBox);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };
            loadButton.Click += (sender, e) => {
                lastWrittenDomContent = ""; // Reset so new content is written
                LoadTabUrl(webView, urlTextBox);
            };

            urlPanel.Controls.Add(urlTextBox, 0, 0);
            urlPanel.Controls.Add(loadButton, 1, 0);

            layout.Controls.Add(urlPanel, 0, 0);
            // Do NOT add webView to tab layout

            tabPage.Controls.Add(layout);
            tabControl.TabPages.Add(tabPage);
            tabControl.SelectedTab = tabPage;

            // Always show the new tab's WebView2 in browserContentPanel
            browserContentPanel.Controls.Clear();
            browserContentPanel.Controls.Add(webView);

            webView.CoreWebView2InitializationCompleted += async (s, e) =>
            {
                if (e.IsSuccess)
                {
                    webView.CoreWebView2.Navigate(initialUrl);
                    // Update tab title after navigation
                    webView.CoreWebView2.NavigationCompleted += async (s2, e2) =>
                    {
                        try
                        {
                            string jsTitle = "document.title";
                            var titleResult = await webView.CoreWebView2.ExecuteScriptAsync(jsTitle);
                            if (!string.IsNullOrWhiteSpace(titleResult))
                            {
                                // Remove quotes from result
                                var pageTitle = System.Text.Json.JsonSerializer.Deserialize<string>(titleResult);
                                if (!string.IsNullOrWhiteSpace(pageTitle))
                                    tabPage.Text = pageTitle.Length > 40 ? pageTitle.Substring(0, 40) + "..." : pageTitle;
                                else
                                    tabPage.Text = "(Untitled)";
                            }
                            else
                            {
                                tabPage.Text = "(Untitled)";
                            }
                        }
                        catch
                        {
                            tabPage.Text = "(Untitled)";
                        }
                    };
                }
            };
            webView.EnsureCoreWebView2Async();

            ShowActiveTabBrowser();
        }

        private void ShowActiveTabBrowser()
        {
            if (tabControl.SelectedTab != null)
            {
                var webView = tabControl.SelectedTab.Tag as WebView2;
                if (webView != null)
                {
                    browserContentPanel.Controls.Clear();
                    browserContentPanel.Controls.Add(webView);
                }
            }
        }

        // Content capture logic
        private void StartContentCapture()
        {
            domCheckTimer = new System.Timers.Timer(5000); // every 5 seconds
            domCheckTimer.Elapsed += (s, e) => CaptureAndWriteContent();
            domCheckTimer.Start();
        }
        private void StopContentCapture()
        {
            if (domCheckTimer != null)
            {
                domCheckTimer.Stop();
                domCheckTimer.Dispose();
                domCheckTimer = null;
            }
        }
        private async void CaptureAndWriteContent()
        {
            if (!isFileWriteActive) return;
            if (tabControl.SelectedTab == null) return;
            var webView = tabControl.SelectedTab.Tag as WebView2;
            if (webView == null || webView.CoreWebView2 == null) return;
            try
            {
                string url = webView.Source?.ToString() ?? "";
                string js = "document.body.innerText";
                var result = await webView.ExecuteScriptAsync(js);
                string content = System.Text.Json.JsonSerializer.Deserialize<string>(result);
                if (string.IsNullOrWhiteSpace(content)) return;
                if (content == lastWrittenDomContent) return;
                lastWrittenDomContent = content;
                string safeUrl = GetSafeFileName(url);
                string dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
                string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WebCaptures", dateFolder);
                Directory.CreateDirectory(folderPath);
                string filePath = Path.Combine(folderPath, safeUrl + ".txt");
                File.WriteAllText(filePath, content);
                // Debug log
                System.Diagnostics.Debug.WriteLine($"[REC] Wrote file: {filePath}");
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"[REC] Error: {ex.Message}");
            }
        }
        private string GetSafeFileName(string url)
        {
            // Sanitize full URL for file name
            foreach (var c in Path.GetInvalidFileNameChars())
                url = url.Replace(c, '_');
            // Remove protocol for brevity
            url = url.Replace("https://", "").Replace("http://", "");
            // Replace '/' and '?' with '_'
            url = url.Replace('/', '_').Replace('?', '_').Replace('&', '_').Replace('=', '_');
            return url.Length > 120 ? url.Substring(0, 120) : url;
        }
    }
}
