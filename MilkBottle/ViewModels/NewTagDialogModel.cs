using System;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class NewTagDialogModel : PropertyChangeBase, IDialogAware {
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

        public void OnDialogOpened( IDialogParameters parameters ) {
            if( parameters != null ) {
                var tagName = parameters.GetValue<string>( cTagNameParameter );

                if(!String.IsNullOrWhiteSpace( tagName )) {
                    Name = tagName;

                    RaisePropertyChanged( () => Name );
                }
            }
        }

        public void OnOk() {
            RaiseRequestClose(
                !String.IsNullOrWhiteSpace( Name )
                    ? new DialogResult( ButtonResult.OK, new DialogParameters {{ cTagNameParameter, Name }} )
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
