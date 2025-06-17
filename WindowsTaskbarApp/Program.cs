using System;
using System.Windows.Forms;
using WindowsTaskbarApp.Tools.Links;
using WindowsTaskbarApp.Tools.MainTool;
using WindowsTaskbarApp.Services.Database;
using WindowsTaskbarApp.Tools.GroupLinks; // Add this using directive if needed

namespace WindowsTaskbarApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize the database
            DatabaseInitializer.Initialize();

            // Initialize tray icons
            MainToolTrayIcon mainToolTrayIcon = null;
            LinksTrayIcon linksTrayIcon = null;
            GroupLinksTrayIcon groupLinksTrayIcon = null;

            Application.ApplicationExit += (s, e) =>
            {
                mainToolTrayIcon?.Dispose();
                linksTrayIcon?.Dispose();
                groupLinksTrayIcon?.Dispose();
            };

            mainToolTrayIcon = new MainToolTrayIcon();
            mainToolTrayIcon.Initialize();

            linksTrayIcon = new LinksTrayIcon();
            linksTrayIcon.Initialize();

            // Instantiate the GroupLinksTrayIcon
            groupLinksTrayIcon = GroupLinksTrayIcon.Instance;
            groupLinksTrayIcon.Initialize();

            // Start the message loop and show the main form
            Application.Run(new Forms.MainForm());
        }
    }
}