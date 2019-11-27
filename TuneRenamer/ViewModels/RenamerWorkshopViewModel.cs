using System;
using System.Collections.ObjectModel;
using System.IO;
using ReusableBits.Mvvm.ViewModelSupport;
using TuneRenamer.Dto;
using TuneRenamer.Interfaces;

namespace TuneRenamer.ViewModels {
    class RenamerWorkshopViewModel : AutomaticCommandBase {
        private readonly IPreferences                       mPreferences;
        private readonly IPlatformDialogService             mDialogService;
        private readonly IPlatformLog                       mLog;
        private readonly ObservableCollection<RenameItem>   mRenameList;
        private string                                      mSourceDirectory;

        public RenamerWorkshopViewModel( IPlatformDialogService dialogService, IPreferences preferences, IPlatformLog log ) {
            mDialogService = dialogService;
            mPreferences = preferences;
            mLog = log;

            mRenameList = new ObservableCollection<RenameItem>();
        }

        public string SourceDirectory {
            get => mSourceDirectory;
            set {
                mSourceDirectory = value;

                mRenameList.Clear();
                RaisePropertyChanged( () => SourceDirectory );
            }
        }

        public void Execute_BrowseSourceFolder() {
            var directory = SourceDirectory;

            if( mDialogService.SelectFolderDialog( "Select Source Directory", ref directory ) == true ) {
                SourceDirectory = directory;
                CollectSource();

                var appPreferences = mPreferences.Load<TuneRenamerPreferences>();

                appPreferences.SourceDirectory = SourceDirectory;
                mPreferences.Save( appPreferences );
            }
        }

        public void Execute_OpenSourceFolder() {
            if( Directory.Exists( SourceDirectory )) {
                try {
                    System.Diagnostics.Process.Start( SourceDirectory );
                }
                catch( Exception ex ) {
                    mLog.LogException( $"OnLaunchRequest:Source Directory: '{SourceDirectory}'", ex );
                }
            }
        }

        public void Execute_RefreshSourceFolder() {
            CollectSource();
        }

        private void CollectSource() { }
    }
}
