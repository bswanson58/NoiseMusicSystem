using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
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
        private string                                      mSourceText;

        public  ObservableCollection<SourceItem>            SourceList => mSourceList;
        public  string                                      SelectedText { get; set; }

        public RenamerWorkshopViewModel( IPlatformDialogService dialogService, IPreferences preferences, IPlatformLog log, ISourceScanner scanner ) {
            mDialogService = dialogService;
            mPreferences = preferences;
            mSourceScanner = scanner;
            mLog = log;

            mSourceList = new ObservableCollection<SourceItem>();

            var appPreferences = mPreferences.Load<TuneRenamerPreferences>();

            SourceDirectory = appPreferences.SourceDirectory;
            CollectSource();
        }

        public string SourceDirectory {
            get => mSourceDirectory;
            set {
                mSourceDirectory = value;

                mSourceList.Clear();
                RaisePropertyChanged( () => SourceDirectory );
            }
        }

        public string SourceText {
            get => mSourceText;
            set {
                mSourceText = value;

                RaisePropertyChanged( () => SourceText );
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
            mSourceList.AddRange( await mSourceScanner.CollectFolder( SourceDirectory, OnItemInspection, OnCopyNames, OnCopyTags ));

            await mSourceScanner.AddTags( mSourceList );
        }

        private void OnCopyNames( SourceFolder folder ) {
            var sourceText = new StringBuilder();

            foreach( var item in folder.Children ) {
                if( item is SourceFile file ) {
                    sourceText.AppendLine( file.Name );
                }
            }

            SourceText = sourceText.ToString();
        }

        private void OnCopyTags( SourceFolder folder ) {
            var sourceText = new StringBuilder();

            foreach( var item in folder.Children ) {
                if( item is SourceFile file ) {
                    sourceText.AppendLine( file.TagName );
                }
            }

            SourceText = sourceText.ToString();
        }

        private void OnItemInspection( SourceFile item ) {
            SourceText = LoadTextFile( item.FileName );
        }

        private string LoadTextFile( string fileName ) {
            var retValue = string.Empty;

            try {
                retValue = File.ReadAllText( fileName );
            }
            catch( Exception ex ) {
                mLog.LogException( $"ItemInspector:LoadTextFile: '{fileName}'", ex );
            }

            return retValue;
        }
    }
}
