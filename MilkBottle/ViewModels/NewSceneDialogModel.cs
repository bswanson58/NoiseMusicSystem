using System;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class NewSceneDialogModel : PropertyChangeBase, IDialogAware {
        public  const string    cSceneNameParameter = "sceneName";

        public  String          Name { get; set; }
        public  string          Title { get; }

        public  DelegateCommand Ok { get; }
        public  DelegateCommand Cancel { get; }

        public  event   Action<IDialogResult> RequestClose;

        public NewSceneDialogModel() {
            Title = "New Scene";

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        public void OnDialogOpened( IDialogParameters parameters ) {
            if( parameters != null ) {
                var sceneName = parameters.GetValue<string>( cSceneNameParameter );

                if(!String.IsNullOrWhiteSpace( sceneName )) {
                    Name = sceneName;

                    RaisePropertyChanged( () => Name );
                }
            }
        }

        public void OnOk() {
            RaiseRequestClose(
                !String.IsNullOrWhiteSpace( Name )
                    ? new DialogResult( ButtonResult.OK, new DialogParameters {{ cSceneNameParameter, Name }} )
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
