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
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // Top bar
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // Address bar
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Browser content


            // Top bar: tabs and plus button (side by side), REC button separate
            var topBar = new TableLayoutPanel();
            topBar.Dock = DockStyle.Fill;
            topBar.Height = 36;
            topBar.ColumnCount = 2;
            topBar.RowCount = 1;
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Tabs + plus
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F)); // REC

            var tabsPanel = new FlowLayoutPanel();
            tabsPanel.Dock = DockStyle.Fill;
            tabsPanel.Height = 32;
            tabsPanel.FlowDirection = FlowDirection.LeftToRight;
            tabsPanel.WrapContents = false;

            tabControl = new TabControl
            {
                Dock = DockStyle.Left,
                Height = 32,
                Width = 800
            };
            tabControl.SelectedIndexChanged += (s, e) => {
                ShowActiveTabBrowser();
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
            plusTabButton.Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold);
            plusTabButton.Size = new System.Drawing.Size(32, 32);
            plusTabButton.Cursor = Cursors.Hand;
            plusTabButton.FlatStyle = FlatStyle.Flat;
            plusTabButton.TabStop = false;
            plusTabButton.Margin = new Padding(0, 0, 0, 0);
            plusTabButton.Click += (sender, e) => AddNewTab("https://www.google.com");

            tabsPanel.Controls.Add(tabControl);
            tabsPanel.Controls.Add(plusTabButton);

            // REC button logic
            recButton = new ToolStripButton("● REC");
            recButton.ForeColor = System.Drawing.Color.Gray;
            recButton.Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);
            recButton.AutoSize = false;
            recButton.Width = 80;
            recButton.Height = 32;
            recButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            recButton.Click += (sender, e) => {
                recBlinkState = !recBlinkState;
                if (recBlinkState)
                {
                    recButton.ForeColor = System.Drawing.Color.Red;
                    recBlinkTimer = new System.Windows.Forms.Timer();
                    recBlinkTimer.Interval = 500;
                    bool dotVisible = true;
                    recBlinkTimer.Tick += (s2, e2) => {
                        dotVisible = !dotVisible;
                        recButton.Text = (dotVisible ? "● " : "  ") + "REC";
                    };
                    recBlinkTimer.Start();
                }
                else
                {
                    if (recBlinkTimer != null)
                    {
                        recBlinkTimer.Stop();
                        recBlinkTimer.Dispose();
                        recBlinkTimer = null;
                    }
                    recButton.ForeColor = System.Drawing.Color.Gray;
                    recButton.Text = "● REC";
                }
                // TODO: Add actual recording logic here
            };

            var recPanel = new Panel { Dock = DockStyle.Fill, Height = 32 };
            var recToolStrip = new ToolStrip { Dock = DockStyle.Fill, GripStyle = ToolStripGripStyle.Hidden, BackColor = System.Drawing.Color.Transparent };
            recToolStrip.Items.Add(recButton);
            recPanel.Controls.Add(recToolStrip);

            topBar.Controls.Add(tabsPanel, 0, 0);
            topBar.Controls.Add(recPanel, 1, 0);

            // Address bar
            var addressBarPanel = new TableLayoutPanel();
            addressBarPanel.Dock = DockStyle.Fill;
            addressBarPanel.ColumnCount = 2;
            addressBarPanel.RowCount = 1;
            addressBarPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            addressBarPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));

            var addressTextBox = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Enter URL here..." };
            var goButton = new Button { Text = "Go", Dock = DockStyle.Fill, Width = 60 };
            goButton.Click += (sender, e) =>
            {
                if (tabControl.SelectedTab != null)
                {
                    var tabPage = tabControl.SelectedTab;
                    var webView = tabPage.Tag as WebView2;
                    if (webView != null)
                    {
                        string urlToNavigate = addressTextBox.Text.Trim();
                        if (string.IsNullOrWhiteSpace(urlToNavigate)) return;
                        if (!urlToNavigate.StartsWith("http://") && !urlToNavigate.StartsWith("https://"))
                            urlToNavigate = "https://" + urlToNavigate;
                        if (Uri.TryCreate(urlToNavigate, UriKind.Absolute, out var uriResult))
                        {
                            if (webView.CoreWebView2 != null)
                            {
                                webView.CoreWebView2.Navigate(uriResult.ToString());
                            }
                            else
                            {
                                webView.CoreWebView2InitializationCompleted += (s2, e2) =>
                                {
                                    webView.CoreWebView2.Navigate(uriResult.ToString());
                                };
                                webView.EnsureCoreWebView2Async();
                            }
                        }
                        // else: invalid URI, do nothing or show error (optional)
                    }
                }
            };
            addressTextBox.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    goButton.PerformClick();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };
            addressBarPanel.Controls.Add(addressTextBox, 0, 0);
            addressBarPanel.Controls.Add(goButton, 1, 0);

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
            var tabPage = new TabPage("New Tab");
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
                Height = 32
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

            urlTextBox.Text = initialUrl;
            urlTextBox.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    LoadTabUrl(webView, urlTextBox);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };
            loadButton.Click += (sender, e) => LoadTabUrl(webView, urlTextBox);

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
            if (tabControl.SelectedTab != null && tabControl.SelectedTab.Controls.Count > 0)
            {
                var layout = tabControl.SelectedTab.Controls[0] as TableLayoutPanel;
                if (layout != null)
                {
                    WebView2 webView = null;
                    foreach (Control c in layout.Controls)
                    {
                        if (c is WebView2)
                        {
                            webView = (WebView2)c;
                            break;
                        }
                    }
                    if (webView != null)
                    {
                        browserContentPanel.Controls.Clear();
                        browserContentPanel.Controls.Add(webView);
                    }
                }
            }
        }
    }
}
