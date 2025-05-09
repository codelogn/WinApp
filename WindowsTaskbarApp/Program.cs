using System;
using System.Threading;
using System.Threading.Tasks;
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

            // Run Main Form, Main Tool Tray App, and Links Tray App independently
            Task.Run(() => RunMainForm());
            Task.Run(() => RunMainToolTrayApp());
            Task.Run(() => RunLinksTrayApp());

            // Keep the main thread alive
            Application.Run();
        }

        private static void RunMainForm()
        {
            Application.Run(new Forms.MainForm());
        }

        private static void RunMainToolTrayApp()
        {
            using (var mainToolTrayIcon = new MainToolTrayIcon())
            {
                mainToolTrayIcon.Initialize();
                Application.Run(); // Keeps the tray app running
            }
        }

        private static void RunLinksTrayApp()
        {
            using (var linksTrayIcon = new LinksTrayIcon())
            {
                linksTrayIcon.Initialize();
                Application.Run(); // Keeps the tray app running
            }
        }
    }
}