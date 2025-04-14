using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace WindowsTaskbarApp.Forms
{
    public partial class ExecutionForm : Form
    {
        private TextBox commandTextBox;
        private ContextMenuStrip contextMenu;

        public ExecutionForm()
        {
            InitializeExecutionForm();
        }

        private void InitializeExecutionForm()
        {
            // Set form properties
            this.Text = "Execution";
            this.Size = new System.Drawing.Size(400, 300);

            // Create a TextBox
            commandTextBox = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ContextMenuStrip = CreateContextMenu()
            };

            // Add MouseDown event to select text on right-click
            commandTextBox.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    commandTextBox.SelectAll();
                }
            };

            // Add the TextBox to the form
            this.Controls.Add(commandTextBox);

            // Load clipboard content into the TextBox when the form loads
            this.Load += ExecutionForm_Load;
        }

        private void ExecutionForm_Load(object sender, EventArgs e)
        {
            // Check if there is text in the clipboard
            if (Clipboard.ContainsText())
            {
                // Set the clipboard text to the TextBox
                commandTextBox.Text = Clipboard.GetText();
            }
        }

        private ContextMenuStrip CreateContextMenu()
        {
            // Create the context menu
            contextMenu = new ContextMenuStrip();

            // Add "Execute in Git Bash" option
            var gitBashMenuItem = new ToolStripMenuItem("Execute in Git Bash");
            gitBashMenuItem.Click += ExecuteInGitBash;
            contextMenu.Items.Add(gitBashMenuItem);

            // Add "Execute in CMD" option
            var cmdMenuItem = new ToolStripMenuItem("Execute in CMD");
            cmdMenuItem.Click += ExecuteInCmd;
            contextMenu.Items.Add(cmdMenuItem);

            return contextMenu;
        }

        private void ExecuteInGitBash(object sender, EventArgs e)
        {
            try
            {
                string commandText = commandTextBox.Text;
                if (!string.IsNullOrWhiteSpace(commandText))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "C:\\Program Files\\Git\\git-bash.exe", // Path to Git Bash
                        Arguments = $"-c \"{commandText}; exec bash\"", // Keep Git Bash open
                        UseShellExecute = false
                    });
                }
                else
                {
                    MessageBox.Show("TextBox is empty or contains invalid text.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to execute in Git Bash: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExecuteInCmd(object sender, EventArgs e)
        {
            try
            {
                string commandText = commandTextBox.Text;
                if (!string.IsNullOrWhiteSpace(commandText))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/k {commandText}", // Keep CMD open
                        UseShellExecute = false
                    });
                }
                else
                {
                    MessageBox.Show("TextBox is empty or contains invalid text.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to execute in CMD: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}