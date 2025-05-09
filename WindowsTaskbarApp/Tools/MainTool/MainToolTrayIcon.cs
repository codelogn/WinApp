using System;
using System.Windows.Forms;

namespace WindowsTaskbarApp.Tools.MainTool
{
    public class MainToolTrayIcon : IDisposable
    {
        private NotifyIcon notifyIcon;

        public void Initialize()
        {
            notifyIcon = new NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Application, // Replace with your custom icon
                Visible = true,
                Text = "Main Tool"
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Open Main Tool", null, (s, e) => OpenMainTool());
            contextMenu.Items.Add("Exit", null, (s, e) => ExitTrayApp());

            notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void OpenMainTool()
        {
            MessageBox.Show("Main Tool opened!", "Main Tool");
        }

        private void ExitTrayApp()
        {
            try
            {
                // Dispose of the tray icon
                Dispose();

                // Exit the message loop for the tray app
                Application.ExitThread();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exiting tray app: {ex.Message}", "Error");
            }
        }

        public void Dispose()
        {
            notifyIcon?.Dispose();
        }
    }
}