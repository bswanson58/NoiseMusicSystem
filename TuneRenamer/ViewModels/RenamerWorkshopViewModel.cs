using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;
using TuneRenamer.Dto;
using TuneRenamer.Interfaces;
using TuneRenamer.Views;

namespace TuneRenamer.ViewModels {
    class RenamerWorkshopViewModel : PropertyChangeBase, IHandle<Events.WindowStateEvent>, IDisposable {
        private readonly IDialogService                     mDialogService;
        private readonly IEventAggregator                   mEventAggregator;
        private readonly IPreferences                       mPreferences;
        private readonly IPlatformDialogService             mPlatformDialogService;
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
        private CharacterPair                               mSelectedCharacterPair;
        private string                                      CurrentText => !String.IsNullOrWhiteSpace( SelectedText ) ? SelectedText : !String.IsNullOrWhiteSpace( SourceText ) ? SourceText : String.Empty;

        public  ObservableCollection<SourceItem>            SourceList { get; }
        public  ObservableCollection<SourceFile>            RenameList { get; }
        public  ObservableCollection<string>                CommonTextList { get; }
        public  ObservableCollection<CharacterPair>         CharacterPairs { get; }
        public  int                                         FileCount { get; private  set; }
        public  int                                         LineCount { get; private set; }
        public  bool                                        CountsMatch {get; private set; }

        public  DelegateCommand                             BrowseSourceFolder { get; }
        public  DelegateCommand                             OpenSourceFolder { get; }
        public  DelegateCommand                             RefreshSourceFolder { get; }

        public  DelegateCommand                             Configuration { get; }
        public  DelegateCommand                             IsolateText { get; }
        public  DelegateCommand                             ClearText { get; }
        public  DelegateCommand                             CleanText { get; }
        public  DelegateCommand                             RestoreText { get; }
        public  DelegateCommand                             FindCommonText { get; }
        public  DelegateCommand                             DeleteCommonText { get; }
        public  DelegateCommand                             DeleteCharacterPair { get; }
        public  DelegateCommand                             RemoveTrailingDigits { get; }
        public  DelegateCommand                             Renumber { get; }
        public  DelegateCommand                             RenameFiles { get; }

        public RenamerWorkshopViewModel( IPlatformDialogService platformDialogService, IPreferences preferences, IPlatformLog log,
                                         ISourceScanner scanner, ITextHelpers textHelpers, IFileRenamer fileRenamer,
                                         IEventAggregator eventAggregator, IDialogService dialogService ) {
            mPlatformDialogService = platformDialogService;
            mDialogService = dialogService;
            mPreferences = preferences;
            mSourceScanner = scanner;
            mTextHelpers = textHelpers;
            mFileRenamer = fileRenamer;
            mEventAggregator = eventAggregator;
            mLog = log;

            Configuration = new DelegateCommand( OnConfiguration );
            BrowseSourceFolder = new DelegateCommand( OnBrowseSourceFolder );
            OpenSourceFolder = new DelegateCommand( OnOpenSourceFolder );
            RefreshSourceFolder = new DelegateCommand( OnRefreshSourceFolder );
            IsolateText = new DelegateCommand( OnIsolateText, CanIsolateText );
            ClearText = new DelegateCommand( OnClearText, CanClearText );
            CleanText = new DelegateCommand( OnCleanText, CanCleanText );
            RestoreText = new DelegateCommand( OnRestoreText, CanRestoreText );
            FindCommonText = new DelegateCommand( OnFindCommonText, CanFindCommonText );
            DeleteCommonText = new DelegateCommand( OnDeleteCommonText, CanDeleteCommonText );
            DeleteCharacterPair = new DelegateCommand( OnDeleteCharacterPair, CanDeleteCharacterPair );
            RemoveTrailingDigits = new DelegateCommand( OnRemoveTrailingDigits, CanRemoveTrailingDigits );
            Renumber = new DelegateCommand( OnRenumber, CanRenumber );
            RenameFiles = new DelegateCommand( OnRenameFiles, CanRenameFiles );

            SourceList = new ObservableCollection<SourceItem>();
            RenameList = new ObservableCollection<SourceFile>();
            CommonTextList = new ObservableCollection<string>();
            CharacterPairs = new ObservableCollection<CharacterPair>();

            CharacterPairs.Add( new CharacterPair( '(', ')', "Between '(' and ')'" ));
            CharacterPairs.Add( new CharacterPair( '[', ']', "Between '[' and ']'" ));
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
                CleanText.RaiseCanExecuteChanged();
                ClearText.RaiseCanExecuteChanged();
                DeleteCharacterPair.RaiseCanExecuteChanged();
                FindCommonText.RaiseCanExecuteChanged();
                Renumber.RaiseCanExecuteChanged();
                RemoveTrailingDigits.RaiseCanExecuteChanged();
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
                DeleteCommonText.RaiseCanExecuteChanged();
            }
        }

        public CharacterPair SelectedCharacterPair {
            get => mSelectedCharacterPair;
            set {
                mSelectedCharacterPair = value;

                DeleteCharacterPair.RaiseCanExecuteChanged();
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

            IsolateText.RaiseCanExecuteChanged();
            RemoveTrailingDigits.RaiseCanExecuteChanged();
        }

        private void OnBrowseSourceFolder() {
            var directory = SourceDirectory;

            if( mPlatformDialogService.SelectFolderDialog( "Select Source Directory", ref directory ) == true ) {
                SourceDirectory = directory;
                CollectSource();

                var appPreferences = mPreferences.Load<TuneRenamerPreferences>();

                appPreferences.SourceDirectory = SourceDirectory;
                mPreferences.Save( appPreferences );
            }
        }

        private void OnOpenSourceFolder() {
            if( Directory.Exists( SourceDirectory )) {
                try {
                    System.Diagnostics.Process.Start( SourceDirectory );
                }
                catch( Exception ex ) {
                    mLog.LogException( $"OnLaunchRequest:Source Directory: '{SourceDirectory}'", ex );
                }
            }
        }

        private void OnRefreshSourceFolder() {
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

        private void OnIsolateText() {
            SourceText = SelectedText;

            IsolateText.RaiseCanExecuteChanged();
            RestoreText.RaiseCanExecuteChanged();
        }

        private bool CanIsolateText() {
            return !String.IsNullOrWhiteSpace( SelectedText );
        }

        private void OnClearText() {
            SourceText = String.Empty;
            SetLineCount();
        }

        private bool CanClearText() {
            return !String.IsNullOrWhiteSpace( SourceText );
        }

        private void OnRestoreText() {
            SourceText = mInitialText;

            IsolateText.RaiseCanExecuteChanged();
            RestoreText.RaiseCanExecuteChanged();
        }

        private bool CanRestoreText() {
            return !String.IsNullOrWhiteSpace( mInitialText );
        }

        private void OnCleanText() {
            CleanSourceText();
        }

        private bool CanCleanText() {
            return !String.IsNullOrWhiteSpace( CurrentText );
        }

        private void OnFindCommonText() {
            if(!String.IsNullOrWhiteSpace( SourceText )) {
                CommonTextList.Clear();
                CommonTextList.AddRange( mTextHelpers.GetCommonSubstring( SourceText, 5 ));

                CommonText = CommonTextList.FirstOrDefault();
            }
        }

        private bool CanFindCommonText() {
            return !String.IsNullOrWhiteSpace( SourceText );
        }

        private void ClearCommonText() {
            CommonTextList.Clear();
            CommonText = String.Empty;
        }

        private void OnDeleteCommonText() {
            SourceText = mTextHelpers.DeleteText( SourceText, CommonText );
        }

        private bool CanDeleteCommonText() {
            return !String.IsNullOrWhiteSpace( CommonText );
        }

        private void OnDeleteCharacterPair() {
            SourceText = mTextHelpers.DeleteText( SourceText, SelectedCharacterPair.StartCharacter, SelectedCharacterPair.EndCharacter );
        }

        private bool CanDeleteCharacterPair() {
            return SelectedCharacterPair != null && !String.IsNullOrWhiteSpace( SourceText );
        }

        private void CleanSourceText() {
            var result = new StringBuilder();
            var lines = mTextHelpers.Lines( CurrentText );
            var defaultIndex = 1;

            foreach( var line in lines ) {
                result.AppendLine( mTextHelpers.ExtendedCleanText( line, defaultIndex ));

                defaultIndex++;
            }

            SourceText = result.ToString();
            RestoreText.RaiseCanExecuteChanged();
        }

        private void OnRemoveTrailingDigits() {
            var result = new StringBuilder();
            var lines = mTextHelpers.Lines( CurrentText );

            foreach( var line in lines ) {
                result.AppendLine( mTextHelpers.RemoveTrailingDigits( line ));
            }

            SourceText = result.ToString();
            RestoreText.RaiseCanExecuteChanged();
        }

        private bool CanRemoveTrailingDigits() {
            return !String.IsNullOrWhiteSpace( CurrentText );
        }

        private void OnRenumber() {
            var result = new StringBuilder();
            var lines = mTextHelpers.Lines( CurrentText );
            var index = 1;

            foreach( var line in lines ) {
                result.AppendLine( mTextHelpers.RenumberIndex( line, index ));

                index++;
            }

            SourceText = result.ToString();
            RestoreText.RaiseCanExecuteChanged();
        }

        private bool CanRenumber() {
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

            RenameFiles.RaiseCanExecuteChanged();
        }

        private async void OnRenameFiles() {
            if( await mFileRenamer.RenameFiles( from file in RenameList where file.WillBeRenamed select file )) {
                RenameList.Clear();
                FileCount = 0;

                CountsMatch = LineCount == FileCount;
                RaisePropertyChanged( () => CountsMatch );

                RenameFiles.RaiseCanExecuteChanged();
            }
        }

        private bool CanRenameFiles() {
            return( LineCount == FileCount ) &&
                  ( FileCount > 0 ) &&
                  ( RenameList.Any( f => f.WillBeRenamed ));
        }

        public void Dispose() {
            mEventAggregator.Unsubscribe( this );
        }

        private void OnConfiguration() {
            var parameters = new DialogParameters();

            mDialogService.ShowDialog( nameof( ReplacementWordsView ), parameters, result => { });
        }
    }
}
