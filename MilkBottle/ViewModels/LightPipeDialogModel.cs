using System;
using MilkBottle.Interfaces;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace MilkBottle.ViewModels {
    class LightPipeDialogModel : IDialogAware {
        public  string                      Title { get; }
        public  DelegateCommand             Ok { get; }
        public  DelegateCommand             Cancel { get; }

        public  event Action<IDialogResult> RequestClose;

        public LightPipeDialogModel( ILightPipePump lightPipe ) {
            Title = "LightPipe";
            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );

            lightPipe.EnableLightPipe( false );
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogOpened( IDialogParameters parameters ) { }

        public void OnDialogClosed() {
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
