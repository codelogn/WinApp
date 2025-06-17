using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowsTaskbarApp.Tools.GroupLinks
{
    public class GroupLinksTrayIcon : IDisposable
    {
        private NotifyIcon notifyIcon;

        private static GroupLinksTrayIcon _instance;
        public static GroupLinksTrayIcon Instance => _instance ??= new GroupLinksTrayIcon();

        public ToolStripMenuItem GroupLinksMenu { get; private set; }

        private GroupLinksTrayIcon()
        {
            GroupLinksMenu = new ToolStripMenuItem("Group links");
            LinksEvents.LinksChanged += (s, e) => RefreshGroupLinksMenu();
        }

        public void Initialize()
        {
            notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = true,
                Text = "Group links"
            };

            var contextMenu = new ContextMenuStrip();

            GroupLinksService.BuildGroupLinksMenu(GroupLinksMenu);

            contextMenu.Items.Add(GroupLinksMenu);

            // Add Exit menu item
            var exitMenuItem = new ToolStripMenuItem("Exit");
            exitMenuItem.Click += ExitMenuItem_Click;
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(exitMenuItem);

            notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Dispose();
            Application.Exit();
        }

        public void RefreshGroupLinksMenu()
        {
            GroupLinksService.BuildGroupLinksMenu(GroupLinksMenu);
        }

        public void Dispose()
        {
            if (notifyIcon != null)
            {
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
                notifyIcon = null;
            }
        }
    }
}