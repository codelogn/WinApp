using System;
using System.Drawing; // Add this for ContentAlignment, Font, and FontStyle
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

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

                // Fetch the HTML content
                var response = await httpClient.GetStringAsync(url);

                // Remove the loading label and add the WebView2 control
                browserForm.Controls.Clear();

                var webView = new Microsoft.Web.WebView2.WinForms.WebView2
                {
                    Dock = DockStyle.Fill
                };

                await webView.EnsureCoreWebView2Async();
                webView.NavigateToString(response); // Set the HTML content

                browserForm.Controls.Add(webView);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error testing URL: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}