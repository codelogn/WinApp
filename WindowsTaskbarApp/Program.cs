using System;
using System.Windows.Forms;
using WindowsTaskbarApp.Tools.Links;
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

            // Initialize the Links tray icon
            using (var linksTrayIcon = new LinksTrayIcon())
            {
                linksTrayIcon.Initialize();

                // Run the main form
                Application.Run(new Forms.MainForm());
            }
        }
    }
}