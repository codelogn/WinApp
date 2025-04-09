using System;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace WindowsTaskbarApp.Forms
{
    public class Form1 : Form
    {
        private TextBox urlTextBox;
        private Button fetchButton;
        private WebView2 webView; // Make WebView2 a class-level field

        public Form1()
        {
            // Initialize the form
            this.Text = "Form 1 - Web Viewer";
            this.Size = new System.Drawing.Size(800, 600);

            // Create a TextBox for the URL
            urlTextBox = new TextBox
            {
                Location = new System.Drawing.Point(10, 10),
                Width = 600
            };

            // Create a Button to fetch the webpage
            fetchButton = new Button
            {
                Text = "Fetch",
                Location = new System.Drawing.Point(620, 10),
                Width = 100
            };
            fetchButton.Click += FetchButton_Click;

            // Create a WebView2 control to display the webpage
            webView = new WebView2
            {
                Location = new System.Drawing.Point(10, 50),
                Size = new System.Drawing.Size(760, 500)
            };

            // Add controls to the form
            this.Controls.Add(urlTextBox);
            this.Controls.Add(fetchButton);
            this.Controls.Add(webView);
        }

        private async void FetchButton_Click(object sender, EventArgs e)
        {
            string url = urlTextBox.Text;

            if (!string.IsNullOrWhiteSpace(url))
            {
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "http://" + url;
                }

                try
                {
                    // Ensure WebView2 is initialized before navigating
                    await webView.EnsureCoreWebView2Async();
                    webView.Source = new Uri(url);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load the webpage. Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid URL.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}