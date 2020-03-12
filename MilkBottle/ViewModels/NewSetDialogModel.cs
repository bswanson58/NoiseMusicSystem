using System;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace MilkBottle.ViewModels {
    class NewSetDialogModel : IDialogAware {
        public  const string    cSetNameParameter = "setName";

        public  String          Name { get; set; }
        public  string          Title { get; }

        public  DelegateCommand Ok { get; }
        public  DelegateCommand Cancel { get; }

        public  event   Action<IDialogResult> RequestClose;

        public NewSetDialogModel() {
            Title = "New Set";

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }
        public void OnDialogOpened( IDialogParameters parameters ) { }

        public void OnOk() {
            RaiseRequestClose(
                !String.IsNullOrWhiteSpace( Name )
                    ? new DialogResult( ButtonResult.OK, new DialogParameters { { cSetNameParameter, Name } } )
                    : new DialogResult( ButtonResult.Cancel ));
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
    }
}
