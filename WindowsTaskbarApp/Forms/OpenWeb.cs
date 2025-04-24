using System;
using System.Net.Http;
using System.Windows.Forms;
using HtmlAgilityPack;
using Microsoft.Web.WebView2.WinForms;

namespace WindowsTaskbarApp.Forms
{
    public class OpenWeb : Form
    {
        private TextBox urlTextBox;
        private TextBox htmlTagTextBox;
        private Button fetchButton;
        private DataGridView resultsGrid;
        private WebView2 htmlViewer;

        public OpenWeb()
        {
            // Initialize the form
            this.Text = "HTML Renderer";
            this.Size = new System.Drawing.Size(1000, 700);

            // Create a TextBox for the URL
            urlTextBox = new TextBox
            {
                Location = new System.Drawing.Point(10, 10),
                Width = 400
            };

            // Create a TextBox for the HTML tag/selector
            htmlTagTextBox = new TextBox
            {
                Location = new System.Drawing.Point(420, 10),
                Width = 200,
                PlaceholderText = "Enter HTML tag (e.g., div, p)"
            };

            // Create a Button to fetch the webpage
            fetchButton = new Button
            {
                Text = "Fetch",
                Location = new System.Drawing.Point(630, 10),
                Width = 100
            };
            fetchButton.Click += FetchButton_Click;

            // Create a DataGridView to display the matching elements
            resultsGrid = new DataGridView
            {
                Location = new System.Drawing.Point(10, 50),
                Size = new System.Drawing.Size(480, 600),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            // Add a custom column to render HTML
            var htmlColumn = new DataGridViewTextBoxColumn
            {
                HeaderText = "HTML Content",
                Name = "HtmlContent",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };
            resultsGrid.Columns.Add(htmlColumn);

            // Create a WebView2 control to render HTML
            htmlViewer = new WebView2
            {
                Location = new System.Drawing.Point(500, 50),
                Size = new System.Drawing.Size(480, 600)
            };

            // Handle row selection to render HTML
            resultsGrid.SelectionChanged += ResultsGrid_SelectionChanged;

            // Add controls to the form
            this.Controls.Add(urlTextBox);
            this.Controls.Add(htmlTagTextBox);
            this.Controls.Add(fetchButton);
            this.Controls.Add(resultsGrid);
            this.Controls.Add(htmlViewer);
        }

        private async void FetchButton_Click(object sender, EventArgs e)
        {
            string url = urlTextBox.Text;
            string htmlTag = htmlTagTextBox.Text;

            if (!string.IsNullOrWhiteSpace(url) && !string.IsNullOrWhiteSpace(htmlTag))
            {
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "http://" + url;
                }

                try
                {
                    // Fetch the webpage content
                    using (HttpClient client = new HttpClient())
                    {
                        string htmlContent = await client.GetStringAsync(url);

                        // Parse the HTML and extract matching elements
                        HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                        htmlDoc.LoadHtml(htmlContent);

                        var matchingNodes = htmlDoc.DocumentNode.SelectNodes($"//{htmlTag}");
                        if (matchingNodes != null)
                        {
                            // Populate the DataGridView with matching elements
                            resultsGrid.Rows.Clear();
                            foreach (var node in matchingNodes)
                            {
                                resultsGrid.Rows.Add(node.OuterHtml.Trim());
                            }
                        }
                        else
                        {
                            MessageBox.Show($"No elements found for the tag '{htmlTag}'.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load the webpage or parse HTML. Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please enter both a valid URL and an HTML tag.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void ResultsGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (resultsGrid.SelectedRows.Count > 0)
            {
                string htmlContent = resultsGrid.SelectedRows[0].Cells["HtmlContent"].Value.ToString();

                // Ensure WebView2 is initialized before navigating
                await htmlViewer.EnsureCoreWebView2Async();

                // Render the selected HTML content
                htmlViewer.NavigateToString(htmlContent);
            }
        }
    }
}