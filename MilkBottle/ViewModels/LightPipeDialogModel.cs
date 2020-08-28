using System;
using MilkBottle.Interfaces;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace MilkBottle.ViewModels {
    class LightPipeDialogModel : IDialogAware {
        private readonly ILightPipePump     mLightPipe;
        private bool                        mLightPipeState;

        public  string                      Title { get; }
        public  DelegateCommand             Ok { get; }
        public  DelegateCommand             Cancel { get; }

        public  event Action<IDialogResult> RequestClose;

        public LightPipeDialogModel( ILightPipePump lightPipe ) {
            mLightPipe = lightPipe;

            Title = "LightPipe";
            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );

            mLightPipeState = mLightPipe.IsEnabled;
            mLightPipe.EnableLightPipe( false );
        }

        public bool LightPipeState {
            get => mLightPipeState;
            set {
                mLightPipeState = value;

                mLightPipe.EnableLightPipe( mLightPipeState );
            }
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogOpened( IDialogParameters parameters ) { }

        public void OnDialogClosed() {
            mLightPipe.Initialize();
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
