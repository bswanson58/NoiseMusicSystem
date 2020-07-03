using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using ReusableBits.Mvvm.ViewModelSupport;
using TuneRenamer.Dto;
using TuneRenamer.Interfaces;

namespace TuneRenamer.ViewModels {
    class RenamerWorkshopViewModel : AutomaticCommandBase, IHandle<Events.WindowStateEvent>, IDisposable {
        private readonly IEventAggregator                   mEventAggregator;
        private readonly IPreferences                       mPreferences;
        private readonly IPlatformDialogService             mDialogService;
        private readonly IPlatformLog                       mLog;
        private readonly ISourceScanner                     mSourceScanner;
        private readonly ITextHelpers                       mTextHelpers;
        private readonly IFileRenamer                       mFileRenamer;
        private SourceItem                                  mSelectedSourceItem;
        private string                                      mSourceDirectory;
        private string                                      mInitialText;
        private string                                      mSourceText;
        private string                                      mSelectedText;
        private string                                      mCommonText;
        private string                                      CurrentText => !String.IsNullOrWhiteSpace( SelectedText ) ? SelectedText : !String.IsNullOrWhiteSpace( SourceText ) ? SourceText : String.Empty;

        public  ObservableCollection<SourceItem>            SourceList { get; }
        public  ObservableCollection<SourceFile>            RenameList { get; }
        public  ObservableCollection<string>                CommonTextList { get; }
        public  ObservableCollection<CharacterPair>         CharacterPairs { get; }
        public  CharacterPair                               SelectedCharacterPair { get; set; }
        public  int                                         FileCount { get; private  set; }
        public  int                                         LineCount { get; private set; }
        public  bool                                        CountsMatch {get; private set; }

        public RenamerWorkshopViewModel( IPlatformDialogService dialogService, IPreferences preferences, IPlatformLog log, ISourceScanner scanner,
                                         ITextHelpers textHelpers, IFileRenamer fileRenamer, IEventAggregator eventAggregator ) {
            mDialogService = dialogService;
            mPreferences = preferences;
            mSourceScanner = scanner;
            mTextHelpers = textHelpers;
            mFileRenamer = fileRenamer;
            mEventAggregator = eventAggregator;
            mLog = log;

            SourceList = new ObservableCollection<SourceItem>();
            RenameList = new ObservableCollection<SourceFile>();
            CommonTextList = new ObservableCollection<string>();
            CharacterPairs = new ObservableCollection<CharacterPair>();

            CharacterPairs.Add( new CharacterPair( '[', ']', "Between '[' and ']'" ));
            CharacterPairs.Add( new CharacterPair( '(', ')', "Between '(' and ')'" ));
            SelectedCharacterPair = CharacterPairs.FirstOrDefault();

            LineCount = 0;
            FileCount = 0;
            CountsMatch = true;

            var appPreferences = mPreferences.Load<TuneRenamerPreferences>();

            SourceDirectory = appPreferences.SourceDirectory;
            CollectSource();

            if( appPreferences.RefreshSourceOnRestore ) {
                mEventAggregator.Subscribe( this );
            }
        }

        public string SourceDirectory {
            get => mSourceDirectory;
            set {
                mSourceDirectory = value;

                SourceList.Clear();
                RaisePropertyChanged( () => SourceDirectory );
            }
        }

        public string SourceText {
            get => mSourceText;
            set {
                mSourceText = value;

                SetLineCount();
                ClearCommonText();
                RaisePropertyChanged( () => SourceText );
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
            foreach( var file in RenameList ) {
                file.IsBeingRenamed = false;
                file.ClearProposedName();
            }
            RenameList.Clear();

            if( SelectedSourceItem is SourceFolder folder ) {
                foreach( var item in folder.Children ) {
                    if( item is SourceFile file ) {
                        if( file.IsRenamable ) {
                            RenameList.Add( file );

                            file.IsBeingRenamed = true;
                        }
                    }
                }
            }

            FileCount = RenameList.Count;
            UpdateProposedFiles();
            RaisePropertyChanged( () => FileCount );

            CountsMatch = LineCount == FileCount;
            RaisePropertyChanged( () => CountsMatch );
        }

        private void OnSelectedTextChanged() {
            SetLineCount();

            RaiseCanExecuteChangedEvent( "CanExecute_IsolateText" );
            RaiseCanExecuteChangedEvent( "CanExecute_RemoveTrailingDigits" );
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

        public void Handle( Events.WindowStateEvent state ) {
            if( state.State != WindowState.Minimized ) {
                CollectSource();
            }
        }

        private async void CollectSource() {
            SourceList.Clear();
            SourceList.AddRange( await mSourceScanner.CollectFolder( SourceDirectory, OnItemInspection, OnCopyNames, OnCopyTags ));

            await mSourceScanner.AddTags( SourceList );
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

        public void Execute_ClearText() {
            SourceText = String.Empty;
            SetLineCount();
        }

        [DependsUpon(nameof( SourceText ))]
        public bool CanExecute_ClearText() {
            return !String.IsNullOrWhiteSpace( SourceText );
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

        [DependsUpon(nameof( SourceText ))]
        public bool CanExecute_CleanText() {
            return !String.IsNullOrWhiteSpace( CurrentText );
        }

        public void Execute_FindCommonText() {
            if(!String.IsNullOrWhiteSpace( SourceText )) {
                CommonTextList.Clear();
                CommonTextList.AddRange( mTextHelpers.GetCommonSubstring( SourceText, 5 ));

                CommonText = CommonTextList.FirstOrDefault();
            }
        }

        [DependsUpon( nameof( SourceText ))]
        public bool CanExecute_FindCommonText() {
            return !String.IsNullOrWhiteSpace( SourceText );
        }

        private void ClearCommonText() {
            CommonTextList.Clear();
            CommonText = String.Empty;
        }

        public void Execute_DeleteCommonText() {
            SourceText = mTextHelpers.DeleteText( SourceText, CommonText );
        }

        [DependsUpon( nameof( CommonText ))]
        public bool CanExecute_DeleteCommonText() {
            return !String.IsNullOrWhiteSpace( CommonText );
        }

        public void Execute_DeleteCharacterPair() {
            SourceText = mTextHelpers.DeleteText( SourceText, SelectedCharacterPair.StartCharacter, SelectedCharacterPair.EndCharacter );
        }

        [DependsUpon( nameof( SourceText ))]
        [DependsUpon( nameof( SelectedCharacterPair ))]
        public bool CanExecute_DeleteCharacterPair() {
            return SelectedCharacterPair != null && !String.IsNullOrWhiteSpace( SourceText );
        }

        private void CleanText() {
            var result = new StringBuilder();
            var lines = mTextHelpers.Lines( CurrentText );
            var defaultIndex = 1;

            foreach( var line in lines ) {
                result.AppendLine( mTextHelpers.ExtendedCleanText( line, defaultIndex ));

                defaultIndex++;
            }

            SourceText = result.ToString();
            RaiseCanExecuteChangedEvent( "CanExecute_RestoreText" );
        }

        public void Execute_RemoveTrailingDigits() {
            var result = new StringBuilder();
            var lines = mTextHelpers.Lines( CurrentText );

            foreach( var line in lines ) {
                result.AppendLine( mTextHelpers.RemoveTrailingDigits( line ));
            }

            SourceText = result.ToString();
            RaiseCanExecuteChangedEvent( "CanExecute_RestoreText" );
        }

        [DependsUpon( nameof( SourceText ))]
        public bool CanExecute_RemoveTrailingDigits() {
            return !String.IsNullOrWhiteSpace( CurrentText );
        }

        public void Execute_Renumber() {
            var result = new StringBuilder();
            var lines = mTextHelpers.Lines( CurrentText );
            var index = 1;

            foreach( var line in lines ) {
                result.AppendLine( mTextHelpers.RenumberIndex( line, index ));

                index++;
            }

            SourceText = result.ToString();
            RaiseCanExecuteChangedEvent( "CanExecute_RestoreText" );
        }

        [DependsUpon( nameof( SourceText ))]
        public bool CanExecute_Renumber() {
            return !String.IsNullOrWhiteSpace( CurrentText );
        }

        private void SetLineCount() {
            LineCount = mTextHelpers.LineCount( CurrentText );
            
            CountsMatch = LineCount == FileCount;
            RaisePropertyChanged( () => CountsMatch );

            UpdateProposedFiles();
            RaisePropertyChanged( () => LineCount );
        }

        private void UpdateProposedFiles() {
            if( LineCount == FileCount ) {
                var proposedNames = mTextHelpers.Lines( CurrentText );
                var index = 0;

                using( var enumerator = proposedNames.GetEnumerator()) {
                    foreach( var file in RenameList ) {
                        if( enumerator.MoveNext()) {
                            file.SetProposedName( mTextHelpers.BasicCleanText( mTextHelpers.SetExtension( file.FileName, enumerator.Current ), index ));

                            index++;
                        }
                    }
                }
            }

            RaiseCanExecuteChangedEvent( "CanExecute_RenameFiles" );
        }

        public async void Execute_RenameFiles() {
            if( await mFileRenamer.RenameFiles( from file in RenameList where file.WillBeRenamed select file )) {
                RenameList.Clear();
                FileCount = 0;

                CountsMatch = LineCount == FileCount;
                RaisePropertyChanged( () => CountsMatch );

                RaiseCanExecuteChangedEvent( "CanExecute_RenameFiles" );
            }
        }

        public bool CanExecute_RenameFiles() {
            return( LineCount == FileCount ) &&
                  ( FileCount > 0 ) &&
                  ( RenameList.Any( f => f.WillBeRenamed ));
        }

        public void Dispose() {
            mEventAggregator.Unsubscribe( this );
        }
    }
}
