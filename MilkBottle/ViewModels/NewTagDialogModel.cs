using System;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace MilkBottle.ViewModels {
    class NewTagDialogModel : IDialogAware {
        public  const string    cTagNameParameter = "tagName";

        public  String          Name { get; set; }
        public  string          Title { get; }

        public  DelegateCommand Ok { get; }
        public  DelegateCommand Cancel { get; }

        public  event   Action<IDialogResult> RequestClose;

        public NewTagDialogModel() {
            Title = "New Tag";

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
                    ? new DialogResult( ButtonResult.OK, new DialogParameters( $"{cTagNameParameter}={Name}" ) )
                    : new DialogResult( ButtonResult.Cancel ) );
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
    }
}
