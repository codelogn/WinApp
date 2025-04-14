using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace WindowsTaskbarApp
{
    public static class ClipboardExecutor
    {
        public static void Execute(string mode)
        {
            try
            {
                string clipboardText = Clipboard.GetText();
                if (string.IsNullOrWhiteSpace(clipboardText))
                {
                    MessageBox.Show("Clipboard is empty or does not contain text.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (mode == "cmd")
                {
                    Process.Start("cmd.exe", $"/k {clipboardText}");
                }
                else if (mode == "gitbash")
                {
                    string gitBashPath = @"C:\Program Files\Git\bin\bash.exe";
                    Process.Start(gitBashPath, $"-c \"{clipboardText}\"");
                }
                else
                {
                    MessageBox.Show("Invalid mode specified.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing clipboard text: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}