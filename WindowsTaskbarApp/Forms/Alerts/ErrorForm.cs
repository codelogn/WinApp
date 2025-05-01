using System;
using System.Windows.Forms;

namespace WindowsTaskbarApp.Forms.Alerts
{
    public class ErrorForm : Form
    {
        public ErrorForm(string errorMessage)
        {
            this.Text = "Error Details";
            this.Size = new System.Drawing.Size(600, 400);

            // Create a TextBox to display the error message
            var textBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                Text = errorMessage,
                ScrollBars = ScrollBars.Vertical
            };

            // Create a "Copy to Clipboard" button
            var copyButton = new Button
            {
                Text = "Copy to Clipboard",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            copyButton.Click += (sender, e) =>
            {
                Clipboard.SetText(errorMessage);
                MessageBox.Show("Error message copied to clipboard.", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            // Add controls to the form
            this.Controls.Add(textBox);
            this.Controls.Add(copyButton);
        }
    }
}