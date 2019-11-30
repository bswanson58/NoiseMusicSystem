using System;
using System.Collections.Generic;
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
        private readonly ITextHelpers                       mTextHelpers;
        private readonly ObservableCollection<SourceItem>   mSourceList;
        private readonly List<SourceFile>                   mRenameList;
        private SourceItem                                  mSelectedSourceItem;
        private string                                      mSourceDirectory;
        private string                                      mInitialText;
        private string                                      mSourceText;
        private string                                      mSelectedText;
        private string                                      mCommonText;
        private string                                      CurrentText => String.IsNullOrWhiteSpace( SelectedText ) ? SourceText : SelectedText;

        public  ObservableCollection<SourceItem>            SourceList => mSourceList;
        public  int                                         FileCount { get; private  set; }
        public  int                                         LineCount { get; private set; }

        public RenamerWorkshopViewModel( IPlatformDialogService dialogService, IPreferences preferences, IPlatformLog log, ISourceScanner scanner, ITextHelpers textHelpers ) {
            mDialogService = dialogService;
            mPreferences = preferences;
            mSourceScanner = scanner;
            mTextHelpers = textHelpers;
            mLog = log;

            mSourceList = new ObservableCollection<SourceItem>();
            mRenameList = new List<SourceFile>();

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

                SetLineCount();
                RaisePropertyChanged( () => SourceText );
                RaiseCanExecuteChangedEvent( "CanExecute_CleanText" );
            }
        }

        public string SelectedText {
            get => mSelectedText;
            set {
                mSelectedText = value;
                
                OnSelectedTextChanged();
            }
        }

        public string CommonText {
            get => mCommonText;
            set {
                mCommonText = value;

                RaisePropertyChanged( () => CommonText );
            }
        }

        public SourceItem SelectedSourceItem {
            get => mSelectedSourceItem;
            set {
                mSelectedSourceItem = value;

                RaisePropertyChanged( () => SelectedSourceItem );
                OnSourceItemSelected();
            }
        }

        private void OnSourceItemSelected() {
            mRenameList.ForEach( f => f.IsBeingRenamed = false );
            mRenameList.Clear();

            if( SelectedSourceItem is SourceFolder folder ) {
                foreach( var item in folder.Children ) {
                    if( item is SourceFile file ) {
                        if( file.IsRenamable ) {
                            mRenameList.Add( file );

                            file.IsBeingRenamed = true;
                        }
                    }
                }
            }

            FileCount = mRenameList.Count;
            RaisePropertyChanged( () => FileCount );
        }

        private void OnSelectedTextChanged() {
            SetLineCount();

            RaiseCanExecuteChangedEvent( "CanExecute_IsolateText" );
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
                    if( file.IsRenamable ) {
                        sourceText.AppendLine( file.Name );
                    }
                }
            }

            SourceText = sourceText.ToString();
            mInitialText = SourceText;
        }

        private void OnCopyTags( SourceFolder folder ) {
            var sourceText = new StringBuilder();

            foreach( var item in folder.Children ) {
                if( item is SourceFile file ) {
                    if( file.IsRenamable ) {
                        sourceText.AppendLine( file.TagName );
                    }
                }
            }

            SourceText = sourceText.ToString();
            mInitialText = SelectedText;
        }

        private void OnItemInspection( SourceFile item ) {
            SourceText = LoadTextFile( item.FileName );
            mInitialText = SourceText;
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

        public void Execute_IsolateText() {
            SourceText = SelectedText;

            RaiseCanExecuteChangedEvent( "CanExecute_IsolateText" );
            RaiseCanExecuteChangedEvent( "CanExecute_RestoreText" );
        }

        public bool CanExecute_IsolateText() {
            return !String.IsNullOrWhiteSpace( SelectedText );
        }

        public void Execute_RestoreText() {
            SourceText = mInitialText;

            RaiseCanExecuteChangedEvent( "CanExecute_IsolateText" );
        }

        public bool CanExecute_RestoreText() {
            return !String.IsNullOrWhiteSpace( mInitialText );
        }

        public void Execute_CleanText() {
            CleanText();
        }

        public bool CanExecute_CleanText() {
            return !String.IsNullOrWhiteSpace( CurrentText );
        }

        public void Execute_FindCommonText() {
            if(!String.IsNullOrWhiteSpace( SourceText )) {
                CommonText = mTextHelpers.GetCommonSubstring( SourceText );
            }
        }

        private void CleanText() {
            var result = new StringBuilder();
            var lines = mTextHelpers.Lines( CurrentText );

            foreach( var line in lines ) {
                result.AppendLine( mTextHelpers.CleanText( line ));
            }

            SourceText = result.ToString();
            RaiseCanExecuteChangedEvent( "CanExecute_RestoreText" );
        }

        private void SetLineCount() {
            LineCount = mTextHelpers.LineCount( CurrentText );

            RaisePropertyChanged( () => LineCount );
        }
    }
}
