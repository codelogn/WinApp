using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsTaskbarApp.Forms
{
    public partial class MainForm : Form
    {
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenu;
        private MenuStrip menuStrip;

        public MainForm()
        {
            // Initialize the form
            this.Text = "Main Application Window";
            this.Size = new Size(800, 600);

            // Create a MenuStrip
            menuStrip = new MenuStrip();

            // Add "File" menu
            var fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("Open Form 1", null, OpenForm1);
            fileMenu.DropDownItems.Add("Text to Speech", null, OpenTextToSpeechForm);
            fileMenu.DropDownItems.Add("Exit", null, (s, e) => Application.Exit());
            menuStrip.Items.Add(fileMenu);

            // Add "Help" menu
            var helpMenu = new ToolStripMenuItem("Help");
            helpMenu.DropDownItems.Add("About", null, ShowAbout);
            menuStrip.Items.Add(helpMenu);

            // Add the MenuStrip to the form
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            // Create a context menu for the NotifyIcon
            contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Open", null, OnOpenClicked);
            contextMenu.Items.Add("Exit", null, OnExitClicked);

            // Create the NotifyIcon
            notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                ContextMenuStrip = contextMenu,
                Visible = true
            };
        }

        private void OpenForm1(object sender, EventArgs e)
        {
            var form1 = new Form1();
            form1.Show();
        }

        private void OpenTextToSpeechForm(object sender, EventArgs e)
        {
            var textToSpeechForm = new TextToSpeechForm();
            textToSpeechForm.Show();
        }

        private void ShowAbout(object sender, EventArgs e)
        {
            MessageBox.Show("This is a sample Windows Forms application.", "About");
        }

        private void OnOpenClicked(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void OnExitClicked(object sender, EventArgs e)
        {
            notifyIcon.Dispose();
            Application.Exit();
        }
    }
}