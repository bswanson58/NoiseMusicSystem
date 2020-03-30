using System;
using System.Windows;
using MilkBottle.Entities;
using Prism.Commands;

namespace MilkBottle.Dto {
    class UiCompanionApp {
        private readonly Action<UiCompanionApp> mCommandAction;

        public FrameworkElement Icon { get; }
        public string           ApplicationName { get; }
        public string           Hint {  get; }
        public DelegateCommand  Command { get; }
        public DateTime         LastHeartbeat { get; private set; }

        public UiCompanionApp( ActiveCompanionApp app, string hint, Action<UiCompanionApp> onCommand ) {
            Icon = app.Icon;
            ApplicationName = app.ApplicationName;
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
