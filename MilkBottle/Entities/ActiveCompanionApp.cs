using System;
using System.Windows;

namespace MilkBottle.Entities {
    class ActiveCompanionApp {
        public FrameworkElement Icon { get; }
        public string           ApplicationName { get; }
        public DateTime         LastHeartbeat { get; private set; }

        public ActiveCompanionApp( string applicationName, FrameworkElement content ) {
            Icon = content;
            ApplicationName = applicationName;

            LastHeartbeat = DateTime.Now;
        }

        public void UpdateHeartbeat() {
            LastHeartbeat = DateTime.Now;
        }
    }
}
