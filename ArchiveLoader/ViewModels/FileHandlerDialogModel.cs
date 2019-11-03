using System;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.ViewModels {
    class FileHandlerDialogModel : AutomaticCommandBase, IDialogAware  {
        public FileHandlerDialogModel() {
            Title = "File Handlers";
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() {
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
        }

        public string Title { get; }
        public event Action<IDialogResult> RequestClose;
    }
}
