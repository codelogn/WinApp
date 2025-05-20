using System;
using System.Windows.Forms;
using WindowsTaskbarApp.Tools.Links;
using WindowsTaskbarApp.Tools.MainTool;
using WindowsTaskbarApp.Services.Database;

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

            Application.ApplicationExit += (s, e) =>
            {
                mainToolTrayIcon?.Dispose();
                linksTrayIcon?.Dispose();
            };

            mainToolTrayIcon = new MainToolTrayIcon();
            mainToolTrayIcon.Initialize();

            linksTrayIcon = new LinksTrayIcon();
            linksTrayIcon.Initialize();

            // Start the message loop and show the main form
            Application.Run(new Forms.MainForm());
        }
    }
}