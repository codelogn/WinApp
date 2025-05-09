using System;
using System.ServiceProcess;
using System.Diagnostics;
using WindowsTaskbarApp.Tools.MainTool;

namespace WindowsTaskbarApp.Services
{
    public partial class MainToolService : ServiceBase
    {
        private EventLog eventLog;

        public MainToolService()
        {
            eventLog = new EventLog();

            if (!EventLog.SourceExists("MainToolServiceSource"))
            {
                EventLog.CreateEventSource("MainToolServiceSource", "MainToolServiceLog");
            }

            eventLog.Source = "MainToolServiceSource";
            eventLog.Log = "MainToolServiceLog";
        }

        protected override void OnStart(string[] args)
        {
            eventLog.WriteEntry("Main Tool Service started.");

            // Initialize the Main Tool tray icon
            var mainToolTrayIcon = new MainToolTrayIcon();
            mainToolTrayIcon.Initialize();
        }

        protected override void OnStop()
        {
            eventLog.WriteEntry("Main Tool Service stopped.");
        }
    }
}