using System;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace Noise.Guide.ViewModels {
    class GuideViewModel : IDialogAware {
        public  DelegateCommand             Close { get; }

        public  string                      Title { get; }
        public  event Action<IDialogResult> RequestClose;

        public GuideViewModel() {
            Close = new DelegateCommand( OnClose );

            Title = "Guide";
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
        }

        private void OnClose() {
            RaiseRequestClose( new DialogResult( ButtonResult.OK, new DialogParameters()));
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
    }
}
