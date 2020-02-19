using System;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace MilkBottle.ViewModels {
    class ConfigurationDialogModel : IDialogAware {

        public string                       Title { get; }
        public DelegateCommand              Ok { get; }
        public DelegateCommand              Cancel { get; }

        public event Action<IDialogResult> RequestClose;

        public ConfigurationDialogModel() {
            Title = "Configuration";
            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        public void OnDialogOpened( IDialogParameters parameters ) {
        }

        public void OnOk() {
            RaiseRequestClose( new DialogResult( ButtonResult.OK ));
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
    }
}
