using System;
using System.Collections.ObjectModel;
using System.Linq;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Ui.Platform;

namespace ArchiveLoader.ViewModels {
    class FileHandlerDialogModel : PropertyChangeBase, IDialogAware  {
        private readonly IPreferences           mPreferences;
        private readonly IPlatformDialogService mPlatformDialogs;
        private FileTypeHandler                 mCurrentHandler;

        public  ObservableCollection<FileTypeHandler>   Handlers { get; }

        public  DelegateCommand             BrowseExe { get; }
        public  DelegateCommand             AddHandler { get; }
        public  DelegateCommand             DeleteHandler { get; }
        public  DelegateCommand             Ok { get; }
        public  DelegateCommand             Cancel { get; }

        public string                       Title { get; }
        public event Action<IDialogResult>  RequestClose;

        public FileHandlerDialogModel( IPreferences preferences, IPlatformDialogService dialogService ) {
            mPreferences = preferences;
            mPlatformDialogs = dialogService;

            BrowseExe = new DelegateCommand( OnBrowseExe, CanBrowseExe );
            AddHandler = new DelegateCommand( OnAddHandler );
            DeleteHandler = new DelegateCommand( OnDeleteHandler, CanDeleteHandler );
            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );

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
                RaisePropertyChanged( () => DeleteInputOnSuccess );
                RaisePropertyChanged( () => TreatStdOutAsError );

                DeleteHandler.RaiseCanExecuteChanged();
                BrowseExe.RaiseCanExecuteChanged();
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

        public bool DeleteInputOnSuccess {
            get => mCurrentHandler?.DeleteInputFileOnSuccess == true;
            set {
                if( mCurrentHandler != null ) {
                    mCurrentHandler.DeleteInputFileOnSuccess = value;
                }
            }
        }

        public bool TreatStdOutAsError {
            get => mCurrentHandler?.TreatStdOutAsError == true;
            set {
                if( mCurrentHandler != null ) {
                    mCurrentHandler.TreatStdOutAsError = value;
                }
            }
        }

        private void OnBrowseExe() {
            if(( mCurrentHandler != null ) &&
               ( mPlatformDialogs.OpenFileDialog( "Select Executable", "*.exe", "Executable files (*.exe, *.bat)|*.exe;*.bat|All files (*.*)|*.*", out var fileName ) == true )) {
                mCurrentHandler.ExePath = fileName;

                RaisePropertyChanged( () => ExePath );
            }
        }

        private bool CanBrowseExe() {
            return mCurrentHandler != null;
        }

        private void OnAddHandler() {
            var handler = new FileTypeHandler { HandlerName = "New Handler" };

            Handlers.Add( handler );
            CurrentHandler = handler;
        }

        private void OnDeleteHandler() {
            if( mCurrentHandler != null ) {
                Handlers.Remove( mCurrentHandler );

                CurrentHandler = null;
            }
        }

        private bool CanDeleteHandler() {
            return mCurrentHandler != null;
        }

        private void OnOk() {
            SaveHandlerList();

            RequestClose?.Invoke( new DialogResult( ButtonResult.OK ));
        }

        private void SaveHandlerList() {
            var handlerList = new FileHandlerStorageList( Handlers );

            mPreferences.Save( handlerList );
        }

        private void OnCancel() {
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
