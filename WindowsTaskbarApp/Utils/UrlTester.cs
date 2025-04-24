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

            if (keywords == null || keywords.Length == 0)
            {
                MessageBox.Show("No keywords provided.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Create a new form with a WebView2 control
                var browserForm = new Form
                {
                    Text = "HTML Viewer - Matched Content",
                    Size = new System.Drawing.Size(800, 600)
                };

                // Add a "Loading" label
                var loadingLabel = new Label
                {
                    Text = "Loading...",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 16, FontStyle.Bold)
                };
                browserForm.Controls.Add(loadingLabel);
                browserForm.Show();

                // Fetch the HTML content from the URL
                using var httpClient = new HttpClient();
                var htmlContent = await httpClient.GetStringAsync(url);

                // Load the HTML content into HtmlAgilityPack
                var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);

                // Extract nodes that match any of the keywords
                var extractedHtml = "<html><body>";
                foreach (var keyword in keywords)
                {
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        var nodes = htmlDoc.DocumentNode.SelectNodes($"//*[contains(translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '{keyword.ToLower()}')]");
                        if (nodes != null)
                        {
                            foreach (var node in nodes)
                            {
                                // Fix anchor links to include absolute URLs
                                var anchorNodes = node.SelectNodes(".//a[@href]");
                                if (anchorNodes != null)
                                {
                                    foreach (var anchor in anchorNodes)
                                    {
                                        var href = anchor.GetAttributeValue("href", string.Empty);
                                        if (!string.IsNullOrEmpty(href) && !Uri.IsWellFormedUriString(href, UriKind.Absolute))
                                        {
                                            var baseUri = new Uri(url);
                                            var absoluteUri = new Uri(baseUri, href);
                                            anchor.SetAttributeValue("href", absoluteUri.ToString());
                                        }
                                    }
                                }

                                extractedHtml += node.OuterHtml + "<hr>";
                            }
                        }
                    }
                }
                extractedHtml += "</body></html>";

                // If no matches are found, show a message and return
                if (extractedHtml == "<html><body></body></html>")
                {
                    MessageBox.Show("No matching content found for the provided keywords.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    browserForm.Close();
                    return;
                }

                // Save the extracted HTML content to a temporary file
                var tempFilePath = Path.Combine(Path.GetTempPath(), "matched_content.html");
                await File.WriteAllTextAsync(tempFilePath, extractedHtml);

                // Initialize the WebView2 control
                var webView = new Microsoft.Web.WebView2.WinForms.WebView2
                {
                    Dock = DockStyle.Fill
                };
                await webView.EnsureCoreWebView2Async();

                // Handle navigation to open links in the default browser
                webView.CoreWebView2.NewWindowRequested += (sender, e) =>
                {
                    e.Handled = true;
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = e.Uri,
                        UseShellExecute = true
                    });
                };

                // Remove the "Loading" label and add the WebView2 control
                browserForm.Controls.Clear();
                browserForm.Controls.Add(webView);

                // Navigate the WebView2 control to the temporary file
                webView.CoreWebView2.Navigate($"file:///{tempFilePath.Replace("\\", "/")}");
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