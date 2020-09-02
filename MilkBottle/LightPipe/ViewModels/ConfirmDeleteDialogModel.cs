using System;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace LightPipe.ViewModels {
    class ConfirmDeleteDialogModel : PropertyChangeBase, IDialogAware {
        public  const string    cEntityNameParameter = "entityName";

        public  String          Name { get; set; }
        public  string          Title { get; }

        public  DelegateCommand Ok { get; }
        public  DelegateCommand Cancel { get; }

        public  event   Action<IDialogResult> RequestClose;

        public ConfirmDeleteDialogModel() {
            Title = "Confirm Delete";

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }
        public void OnDialogOpened( IDialogParameters parameters ) {
            Name = parameters.GetValue<string>( cEntityNameParameter );

            RaisePropertyChanged( () => Name );
        }

        public void OnOk() {
            RaiseRequestClose(new DialogResult( ButtonResult.OK, new DialogParameters()));
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
    }
}
