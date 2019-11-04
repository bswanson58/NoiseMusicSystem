using System;
using ArchiveLoader.Interfaces;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.ViewModels {
    class FileHandlerDialogModel : AutomaticCommandBase, IDialogAware  {
        private readonly IPreferences       mPreferences;

        public string                       Title { get; }
        public event Action<IDialogResult>  RequestClose;

        public FileHandlerDialogModel( IPreferences preferences ) {
            mPreferences = preferences;

            Title = "File Handlers";
        }

        public void Execute_OnOk() {
            RequestClose?.Invoke( new DialogResult( ButtonResult.OK ));
        }

        public void Execute_OnCancel() {
            RequestClose?.Invoke( new DialogResult( ButtonResult.OK ));
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }
        public void OnDialogOpened( IDialogParameters parameters ) { }
    }
}
