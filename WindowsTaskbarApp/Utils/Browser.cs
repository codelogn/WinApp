using System;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace WindowsTaskbarApp.Utils
{
    public static class Browser
    {
        /// <summary>
        /// Opens the specified URL in the default web browser.
        /// </summary>
        /// <param name="url">The URL to open.</param>
        public static void OpenInDefaultBrowser(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("The URL is empty. Please provide a valid URL.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Validate the URL format
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult) ||
                    (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                {
                    MessageBox.Show("The URL is not valid. Please enter a valid HTTP or HTTPS URL.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Open the URL in the default browser
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while trying to open the URL: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Opens the specified URL in an embedded browser window using WebView2.
        /// </summary>
        /// <param name="url">The URL to open.</param>
        public static void OpenInEmbeddedBrowser(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("The URL is empty. Please provide a valid URL.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Validate the URL format
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult) ||
                    (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                {
                    MessageBox.Show("The URL is not valid. Please enter a valid HTTP or HTTPS URL.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Create a new form to host the WebView2 control
                var browserForm = new Form
                {
                    Text = "Embedded Browser",
                    Size = new System.Drawing.Size(800, 600),
                    StartPosition = FormStartPosition.CenterScreen
                };

                var webView = new WebView2
                {
                    Dock = DockStyle.Fill
                };

                browserForm.Controls.Add(webView);

                // Initialize WebView2 and navigate to the URL
                webView.Source = new Uri(url);

                browserForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while trying to open the embedded browser: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}