using System;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using System.IO;
using System.Timers;
using System.Linq;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Text.Json;

namespace WindowsTaskbarApp.Tools.MiniWebBrowser
{
    public partial class WebBrowserForm : Form
    {
        private WebView2 webView;
        private TextBox urlTextBox;
        private Button loadButton;
        private ToolStrip browserToolStrip;

        private bool isFileWriteActive = false;
        private string fileWritePath = null;
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

            // Start recording by default
            isFileWriteActive = false;
            recButton.PerformClick();
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
                    recBlinkTimer.Stop();
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
            return !!(el.offsetWidth || el.offsetHeight || el.getClientRects().length) && getComputedStyle(el).visibility !== \"hidden\" && getComputedStyle(el).display !== \"none\";
        }
        function getTextContent() {
            let result = \"\";
            // Title
            let title = document.title || \"\";
            if (title) result += \"TITLE: \" + title + "\n";
            // Meta description
            let desc = \"\";
            let metaDesc = document.querySelector(\"meta[name=description]\");
            if (metaDesc && metaDesc.content) desc = metaDesc.content.trim();
            if (desc) result += \"DESCRIPTION: \" + desc + "\n";
            // Topic (try meta keywords, og:title, og:topic, etc.)
            let topic = \"\";
            let metaTopic = document.querySelector(\"meta[name=keywords], meta[property=og:title], meta[property=og:topic]\");
            if (metaTopic && metaTopic.content) topic = metaTopic.content.trim();
            if (topic) result += \"TOPIC: \" + topic + "\n";
            // Main blocks
            let mainBlocks = Array.from(document.querySelectorAll(\"[role=main], main, article, section, .main-content, .content, .story, .card, .section\"));
            let used = new Set();
            mainBlocks.forEach((block, idx) => {
                if (isVisible(block) && !used.has(block)) {
                    used.add(block);
                    let heading = block.querySelector(\"h1,h2,h3,h4,h5,h6\");
                    let blockTitle = heading ? heading.innerText.trim() : \"\";
                    let summaryEl = block.querySelector(\".summary, .desc, .byline, .subtitle\");
                    let summary = summaryEl ? summaryEl.innerText.trim() : \"\";
                    let text = block.innerText.trim();
                    result += \"MAINBLOCK: \" + (blockTitle ? blockTitle : \"\") + "\n";
                    if (summary) result += \"SUMMARY: \" + summary + "\n";
                    result += \"CONTENT: \" + text + "\n";
                }
            });
            // Fallback: single main
            let main = document.querySelector(\"[role=main], main, article, section\");
            if (main && isVisible(main) && !used.has(main)) {
                result += \"MAIN: \" + main.innerText.trim() + "\n";
            }
            // Headings
            document.querySelectorAll(\"h1,h2,h3,h4,h5,h6\").forEach(h => {
                if (isVisible(h)) result += h.tagName + \": \" + h.innerText.trim() + "\n";
                                string js = @"
    (() => {
        function isVisible(el) {
            return !!(el.offsetWidth || el.offsetHeight || el.getClientRects().length) && getComputedStyle(el).visibility !== ""hidden"" && getComputedStyle(el).display !== ""none"";
        }
        function getTextContent() {
            let result = '';
            // Title
            let title = document.title || '';
            if (title) result += 'TITLE: ' + title + '\n';
            // Meta description
            let desc = '';
            let metaDesc = document.querySelector('meta[name=description]');
            if (metaDesc && metaDesc.content) desc = metaDesc.content.trim();
            if (desc) result += 'DESCRIPTION: ' + desc + '\n';
            // Topic (try meta keywords, og:title, og:topic, etc.)
            let topic = '';
            let metaTopic = document.querySelector('meta[name=keywords], meta[property=og:title], meta[property=og:topic]');
            if (metaTopic && metaTopic.content) topic = metaTopic.content.trim();
            if (topic) result += 'TOPIC: ' + topic + '\n';
            // Main blocks
            let mainBlocks = Array.from(document.querySelectorAll('[role=main], main, article, section, .main-content, .content, .story, .card, .section'));
            let used = new Set();
            mainBlocks.forEach((block, idx) => {
                if (isVisible(block) && !used.has(block)) {
                    used.add(block);
                    let heading = block.querySelector('h1,h2,h3,h4,h5,h6');
                    let blockTitle = heading ? heading.innerText.trim() : '';
                    let summaryEl = block.querySelector('.summary, .desc, .byline, .subtitle');
                    let summary = summaryEl ? summaryEl.innerText.trim() : '';
                    let text = block.innerText.trim();
                    result += 'MAINBLOCK: ' + (blockTitle ? blockTitle : '') + '\n';
                    if (summary) result += 'SUMMARY: ' + summary + '\n';
                    result += 'CONTENT: ' + text + '\n';
                }
            });
            // Fallback: single main
            let main = document.querySelector('[role=main], main, article, section');
            if (main && isVisible(main) && !used.has(main)) {
                result += 'MAIN: ' + main.innerText.trim() + '\n';
            }
            // Headings
            document.querySelectorAll('h1,h2,h3,h4,h5,h6').forEach(h => {
                if (isVisible(h)) result += h.tagName + ': ' + h.innerText.trim() + '\n';
            });
            // Paragraphs
            document.querySelectorAll('p').forEach(p => {
                if (isVisible(p)) result += 'P: ' + p.innerText.trim() + '\n';
            });
            // General text blocks (exclude code/script/style)
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
            // Tables
            document.querySelectorAll('table').forEach(table => {
                if (isVisible(table)) {
                    let rows = table.querySelectorAll('tr');
                    result += 'TABLE:\n';
                    rows.forEach(row => {
                        let cells = row.querySelectorAll('th,td');
                        let rowText = Array.from(cells).map(cell => cell.innerText.trim()).join(' | ');
                        result += rowText + '\n';
                    });
                }
            });
            // Links
            document.querySelectorAll('a[href]').forEach(a => {
                if (isVisible(a)) result += 'LINK: ' + a.innerText.trim() + ' -> ' + a.href + '\n';
            });
            return result;
        }
        return getTextContent();
    })();
";
";
            document.querySelectorAll('table').forEach(table => {
                if (isVisible(table)) {
                    let rows = table.querySelectorAll('tr');
                    result += 'TABLE:\n';
                    rows.forEach(row => {
                        let cells = row.querySelectorAll('th,td');
                        let rowText = Array.from(cells).map(cell => cell.innerText.trim()).join(' | ');
                        result += rowText + '\n';
                    });
                }
            });
            // Links
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
                                var filePath = Path.Combine(dateFolder, safeFileName + ".txt");
                                currentUrlFilePath = filePath;
                                if (currentDomContent != lastWrittenDomContent)
                                {
                                    var yamlSerializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
                                    var yamlObj = new WebContentRecord
                                    {
                                        Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                        Url = url,
                                        Content = ParseStructuredContent(currentDomContent)
                                    };
                                    var yaml = yamlSerializer.Serialize(yamlObj);
                                    File.WriteAllText(filePath, yaml);
                                    lastWrittenDomContent = currentDomContent;
                                }
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

        private string GetSafeFileName(string url)
        {
            // Remove invalid filename characters and limit length
            var invalidChars = Path.GetInvalidFileNameChars();
            var safe = new string(url.ToCharArray().Where(c => !invalidChars.Contains(c)).ToArray());
            if (safe.Length > 100) safe = safe.Substring(0, 100); // Limit length
            return safe;
        }

        private string GetDifference(string oldContent, string newContent)
        {
            if (string.IsNullOrEmpty(oldContent)) return newContent;
            var oldLines = oldContent.Split('\n');
            var newLines = newContent.Split('\n');
            var diffLines = new System.Collections.Generic.List<string>();
            foreach (var line in newLines)
            {
                if (!string.IsNullOrWhiteSpace(line) && Array.IndexOf(oldLines, line) < 0)
                    diffLines.Add(line);
            }
            return string.Join("\n", diffLines);
        }

        public class WebContentRecord
        {
            public string Timestamp { get; set; }
            public string Url { get; set; }
            public List<IContentSection> Content { get; set; }
        }

        // Strongly-typed section classes for YAML
        public interface IContentSection { }
        public class TitleSection : IContentSection
        {
            public string Type { get; set; } = "title";
            public string Content { get; set; }
        }
        public class TopicSection : IContentSection
        {
            public string Type { get; set; } = "topic";
            public string Content { get; set; }
        }
        public class DescriptionSection : IContentSection
        {
            public string Type { get; set; } = "description";
            public string Content { get; set; }
        }
        public class GeneralTextSection : IContentSection
        {
            public string Type { get; set; } = "generaltext";
            public List<string> Content { get; set; } = new List<string>();
        }
        // ...existing code...
        public class MainSection : IContentSection
        {
            public string Type { get; set; } = "main";
            public string Content { get; set; }
        }
        public class TableSection : IContentSection
        {
            public string Type { get; set; } = "table";
            public List<Dictionary<string, string>> Content { get; set; } = new List<Dictionary<string, string>>();
        }
        public class TextSection : IContentSection
        {
            public string Type { get; set; } = "text";
            public List<string> Content { get; set; } = new List<string>();
        }
        public class LinksSection : IContentSection
        {
            public string Type { get; set; } = "links";
            public List<LinkItem> Content { get; set; } = new List<LinkItem>();
        }
        public class LinkItem
        {
            public string Text { get; set; }
            public string Url { get; set; }
        }

        // Helper to parse JS extracted content into a hierarchical object for YAML
        private List<IContentSection> ParseStructuredContent(string content)
        {
            var lines = content.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
            var result = new List<IContentSection>();
            IContentSection currentSection = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("TITLE: "))
                {
                    result.Add(new TitleSection { Content = line.Substring(7).Trim() });
                }
                else if (line.StartsWith("TOPIC: "))
                {
                    result.Add(new TopicSection { Content = line.Substring(7).Trim() });
                }
                else if (line.StartsWith("DESCRIPTION: "))
                {
                    result.Add(new DescriptionSection { Content = line.Substring(12).Trim() });
                }
                else if (line.StartsWith("MAINBLOCK: "))
                {
                    if (currentSection != null)
                        result.Add(currentSection);
                    var title = line.Substring(10).Trim();
                    string summary = null;
                    string blockContent = null;
                    int idx = lines.IndexOf(line);
                    if (idx + 1 < lines.Count && lines[idx + 1].StartsWith("SUMMARY: "))
                    {
                        summary = lines[idx + 1].Substring(9).Trim();
                    }
                    if (idx + 2 < lines.Count && lines[idx + 2].StartsWith("CONTENT: "))
                    {
                        blockContent = lines[idx + 2].Substring(9).Trim();
                    }
                    result.Add(new MainSection { Content = (title + (summary != null ? "\n" + summary : "") + (blockContent != null ? "\n" + blockContent : "")).Trim() });
                    currentSection = null;
                }
                else if (line.StartsWith("MAIN: "))
                {
                    if (currentSection != null)
                        result.Add(currentSection);
                    currentSection = new MainSection { Content = line.Substring(6).Trim() };
                }
                else if (line.StartsWith("TABLE:"))
                {
                    if (currentSection != null)
                        result.Add(currentSection);
                    currentSection = new TableSection();
                }
                else if (line.Contains(" | "))
                {
                    if (currentSection is TableSection tableSection)
                    {
                        var row = line.Split(new[] { " | " }, StringSplitOptions.None);
                        var rowDict = new Dictionary<string, string>();
                        for (int i = 0; i < row.Length; i++)
                        {
                            rowDict[$"column{i + 1}"] = row[i].Trim();
                        }
                        tableSection.Content.Add(rowDict);
                    }
                }
                else if (line.StartsWith("P: "))
                {
                    if (!(currentSection is TextSection))
                    {
                        if (currentSection != null)
                            result.Add(currentSection);
                        currentSection = new TextSection();
                    }
                    ((TextSection)currentSection).Content.Add(line.Substring(3).Trim());
                }
                else if (line.StartsWith("GENERALTEXT: "))
                {
                    if (!(currentSection is GeneralTextSection))
                    {
                        if (currentSection != null)
                            result.Add(currentSection);
                        currentSection = new GeneralTextSection();
                    }
                    ((GeneralTextSection)currentSection).Content.Add(line.Substring(13).Trim());
                }
                else if (line.StartsWith("LINK: "))
                {
                    if (!(currentSection is LinksSection))
                    {
                        if (currentSection != null)
                            result.Add(currentSection);
                        currentSection = new LinksSection();
                    }
                    var linkParts = line.Substring(6).Trim().Split(new[] { " -> " }, 2, StringSplitOptions.None);
                    if (linkParts.Length == 2)
                    {
                        ((LinksSection)currentSection).Content.Add(new LinkItem
                        {
                            Text = linkParts[0].Trim(),
                            Url = linkParts[1].Trim()
                        });
                    }
                }
            }

            if (currentSection != null)
                result.Add(currentSection);

            return result;
        }
    }
}
