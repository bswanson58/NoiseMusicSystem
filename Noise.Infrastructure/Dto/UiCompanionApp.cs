using System;
using System.Windows;
using Microsoft.Practices.Prism.Commands;

namespace Noise.Infrastructure.Dto {
    public class UiCompanionApp {
        private readonly Action<UiCompanionApp> mCommandAction;

        public FrameworkElement Icon { get; }
        public string           ApplicationName { get; }
        public string           Hint {  get; }
        public DelegateCommand  Command { get; }
        public DateTime         LastHeartbeat { get; private set; }

        public UiCompanionApp( string applicationName, FrameworkElement content, string hint, Action<UiCompanionApp> onCommand ) {
            Icon = content;
            ApplicationName = applicationName;
            Hint = hint;

            mCommandAction = onCommand;
            Command = new DelegateCommand( OnCommand );

            LastHeartbeat = DateTime.Now;
        }

        public void UpdateHeartbeat() {
            LastHeartbeat = DateTime.Now;
        }

        private void OnCommand() {
            mCommandAction?.Invoke( this );
        }
    }
}
