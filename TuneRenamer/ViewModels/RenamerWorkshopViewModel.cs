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
        private readonly ISourceScanner                     mSourceScanner;
        private readonly ObservableCollection<SourceItem>   mSourceList;
        private string                                      mSourceDirectory;

        public  ObservableCollection<SourceItem>            SourceList => mSourceList;

        public RenamerWorkshopViewModel( IPlatformDialogService dialogService, IPreferences preferences, IPlatformLog log, ISourceScanner scanner ) {
            mDialogService = dialogService;
            mPreferences = preferences;
            mSourceScanner = scanner;
            mLog = log;

            mSourceList = new ObservableCollection<SourceItem>();

            var appPreferences = mPreferences.Load<TuneRenamerPreferences>();

            SourceDirectory = appPreferences.SourceDirectory;
        }

        public string SourceDirectory {
            get => mSourceDirectory;
            set {
                mSourceDirectory = value;

                mSourceList.Clear();
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

        private async void CollectSource() {
            mSourceList.Clear();
            mSourceList.AddRange( await mSourceScanner.CollectFolder( SourceDirectory, OnItemInspection ));

            await mSourceScanner.AddTags( mSourceList );
        }

        private void OnItemInspection( SourceItem item ) { }
    }
}
