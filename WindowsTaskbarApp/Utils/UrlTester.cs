using System;
using System.Drawing; // Add this for ContentAlignment, Font, and FontStyle
using System.IO; // Add this for Path and File
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic; // Add this for List
using System.Text.RegularExpressions; // Add this for Regex
using HtmlAgilityPack; // Add this for HTML parsing

namespace WindowsTaskbarApp.Utils
{
    public static class UrlTester
    {
        public static async Task TestUrlAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("URL is empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Encode the URL to handle special characters
                var encodedUrl = Uri.EscapeUriString(url);

                using var httpClient = new HttpClient();

                // Create a new form with a WebView2 control to render the HTML
                var browserForm = new Form
                {
                    Text = $"HTML Viewer - {url}",
                    Size = new System.Drawing.Size(800, 600)
                };

                // Add a loading label
                var loadingLabel = new Label
                {
                    Text = "Loading...",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 16, FontStyle.Bold)
                };
                browserForm.Controls.Add(loadingLabel);

                // Show the form before starting the fetch
                browserForm.Show();

                // Initialize the WebView2 control
                var webView = new Microsoft.Web.WebView2.WinForms.WebView2
                {
                    Dock = DockStyle.Fill
                };

                await webView.EnsureCoreWebView2Async();

                // Remove the loading label and add the WebView2 control
                browserForm.Controls.Clear();
                browserForm.Controls.Add(webView);

                // Navigate to the URL
                webView.CoreWebView2.Navigate(encodedUrl);
            }
            catch (Exception ex)
            {
                // Create a detailed error message with a "Copy to Clipboard" option
                var errorMessage = $"Error testing URL ({url}): {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
                var result = MessageBox.Show(
                    $"{errorMessage}\n\nDo you want to copy this error to the clipboard?",
                    "Error",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error
                );

                if (result == DialogResult.Yes)
                {
                    Clipboard.SetText(errorMessage); // Copy the error message to the clipboard
                }
            }
        }

        public static async Task TestUrlWithKeywordsAsync(string url, string[] keywords)
        {
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("URL is empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Create a new form with a WebView2 control
                var browserForm = new Form
                {
                    Text = $"HTML Viewer - Extracted Parts",
                    Size = new System.Drawing.Size(800, 600)
                };

                var webView = new Microsoft.Web.WebView2.WinForms.WebView2
                {
                    Dock = DockStyle.Fill
                };

                await webView.EnsureCoreWebView2Async();
                webView.CoreWebView2.Navigate(url);

                browserForm.Controls.Add(webView);
                browserForm.Show();

                // Wait for the page to load
                await Task.Delay(5000); // Adjust the delay as needed

                // Get the rendered HTML
                var renderedHtml = await webView.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML");
                renderedHtml = System.Text.Json.JsonSerializer.Deserialize<string>(renderedHtml); // Decode the JSON string

                // Load the rendered HTML into HtmlAgilityPack
                var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(renderedHtml);

                // Extract the parent nodes containing the matching keywords
                var extractedHtml = "<html><body>";
                foreach (var keyword in keywords)
                {
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        var nodes = htmlDoc.DocumentNode.SelectNodes($"//*[contains(text(), '{keyword}')]");
                        if (nodes != null)
                        {
                            foreach (var node in nodes)
                            {
                                extractedHtml += node.OuterHtml + "<hr>";
                            }
                        }
                    }
                }
                extractedHtml += "</body></html>";

                // Save the extracted HTML content to a temporary file
                var tempFilePath = Path.Combine(Path.GetTempPath(), "extracted_content.html");
                await File.WriteAllTextAsync(tempFilePath, extractedHtml);

                // Navigate to the temporary file
                webView.CoreWebView2.Navigate(tempFilePath);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error testing URL ({url}): {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
                var result = MessageBox.Show(
                    $"{errorMessage}\n\nDo you want to copy this error to the clipboard?",
                    "Error",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error
                );

                if (result == DialogResult.Yes)
                {
                    Clipboard.SetText(errorMessage);
                }
            }
        }
    }
}