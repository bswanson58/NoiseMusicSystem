using System;
using System.Collections.ObjectModel;
using System.Linq;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;
using ArchiveLoader.Platform;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.ViewModels {
    class FileHandlerDialogModel : AutomaticCommandBase, IDialogAware  {
        private readonly IPreferences           mPreferences;
        private readonly IPlatformDialogService mPlatformDialogs;
        private FileTypeHandler                 mCurrentHandler;

        public  ObservableCollection<FileTypeHandler>   Handlers { get; }

        public string                       Title { get; }
        public event Action<IDialogResult>  RequestClose;

        public FileHandlerDialogModel( IPreferences preferences, IPlatformDialogService dialogService ) {
            mPreferences = preferences;
            mPlatformDialogs = dialogService;

            Title = "File Handlers";
            Handlers = new ObservableCollection<FileTypeHandler>();
        }

        public FileTypeHandler CurrentHandler {
            get => mCurrentHandler;
            set {
                mCurrentHandler = value;

                RaisePropertyChanged( () => HandlerName );
                RaisePropertyChanged( () => InputExtension );
                RaisePropertyChanged( () => OutputExtension );
                RaisePropertyChanged( () => ExePath );
                RaisePropertyChanged( () => CommandArguments );

                RaiseCanExecuteChangedEvent( "CanExecute_DeleteHandler" );
                RaiseCanExecuteChangedEvent( "CanExecute_BrowseExe" );
            }
        }

        public string HandlerName {
            get => mCurrentHandler?.HandlerName;
            set {
                if( mCurrentHandler != null ) {
                    mCurrentHandler.HandlerName = value;
                }

                RaisePropertyChanged( () => HandlerName );
            }
        }

        public string InputExtension {
            get => mCurrentHandler?.InputExtension;
            set {
                if( mCurrentHandler != null ) {
                    mCurrentHandler.InputExtension = value;
                }
            }
        }

        public string OutputExtension {
            get => mCurrentHandler?.OutputExtension;
            set {
                if( mCurrentHandler != null ) {
                    mCurrentHandler.OutputExtension = value;
                }
            }
        }

        public string ExePath {
            get => mCurrentHandler?.ExePath;
            set {
                if( mCurrentHandler != null ) {
                    mCurrentHandler.ExePath = value;
                }
            }
        }

        public string CommandArguments {
            get => mCurrentHandler?.CommandArguments;
            set {
                if( mCurrentHandler != null ) {
                    mCurrentHandler.CommandArguments = value;
                }
            }
        }

        public void Execute_BrowseExe() {
            if(( mCurrentHandler != null ) &&
               ( mPlatformDialogs.OpenFileDialog( "Select Executable", "*.exe", "Executable files (*.exe, *.bat)|*.exe;*.bat|All files (*.*)|*.*", out var fileName ) == true )) {
                mCurrentHandler.ExePath = fileName;

                RaisePropertyChanged( () => ExePath );
            }
        }

        public bool CanExecute_BrowseExe() {
            return mCurrentHandler != null;
        }

        public void Execute_AddHandler() {
            var handler = new FileTypeHandler { HandlerName = "New Handler" };

            Handlers.Add( handler );
            CurrentHandler = handler;
        }

        public void Execute_DeleteHandler() {
            if( mCurrentHandler != null ) {
                Handlers.Remove( mCurrentHandler );

                CurrentHandler = null;
            }
        }

        public bool CanExecute_DeleteHandler() {
            return mCurrentHandler != null;
        }

        public void Execute_OnOk() {
            SaveHandlerList();

            RequestClose?.Invoke( new DialogResult( ButtonResult.OK ));
        }

        private void SaveHandlerList() {
            var handlerList = new FileHandlerStorageList( Handlers );

            mPreferences.Save( handlerList );
        }

        public void Execute_OnCancel() {
            RequestClose?.Invoke( new DialogResult( ButtonResult.OK ));
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }
        public void OnDialogOpened( IDialogParameters parameters ) {
            var handlers = mPreferences.Load<FileHandlerStorageList>();

            Handlers.Clear();
            Handlers.AddRange( handlers.Handlers );

            CurrentHandler = Handlers.FirstOrDefault();
        }
    }
}
