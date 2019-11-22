using System;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using Album4Matter.Dto;
using Album4Matter.Interfaces;
using Album4Matter.Platform;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Album4Matter.ViewModels {
    class ItemInspectionViewModel : AutomaticCommandBase, IItemInspectionViewModel {
        private readonly string[]                       mTextFileExtensions = { ".txt", ".nfo" };
        private readonly string[]                       mMusicFileExtensions = { ".mp3", ".flac" };

        private string                                  mArtistTag;
        private string                                  mAlbumTag;
        private string                                  mDateTag;

        private readonly Subject<InspectionItemUpdate>  mInspectionChangedSubject;
        private readonly IPlatformLog                   mLog;
        private string                                  mSelectedText;

        public IObservable<InspectionItemUpdate>        InspectionItemChanged => mInspectionChangedSubject;

        public  string                                  InspectionItemName { get; private set; }
        public  string                                  InspectionText { get; set; }

        public ItemInspectionViewModel( IPlatformLog log ) {
            mLog = log;
            mInspectionChangedSubject = new Subject<InspectionItemUpdate>();
        }

        public void ClearInspectionItem() {
            InspectionItemName = String.Empty;
            InspectionText = String.Empty;

            RaisePropertyChanged( () => InspectionItemName );
            RaisePropertyChanged( () => InspectionText );

            SelectedText = String.Empty;

            ClearTags();
            RaiseCanExecuteChangedEvent("CanExecute_UseTags");
        }

        public void SetInspectionItem( SourceItem item ) {
            InspectionItemName = item.Name;

            InspectItem( item );

            RaisePropertyChanged( () => InspectionItemName );
            RaisePropertyChanged( () => InspectionText );
        }

        public string SelectedText {
            get => mSelectedText;
            set {
                mSelectedText = value;

                RaiseCanExecuteChangedEvent( "CanExecute_TextIsArtist" );
                RaiseCanExecuteChangedEvent( "CanExecute_TextIsAlbum" );
                RaiseCanExecuteChangedEvent( "CanExecute_TextIsDate" );
            }
        }

        private void InspectItem( SourceItem item ) {
            ClearTags();

            if( ItemIsTextFile( item )) {
                InspectionText = LoadTextFile( item.FileName );
            }
            else if( ItemIsMusicFile( item )) {
                InspectionText = LoadMusicTags( item );

                RaiseCanExecuteChangedEvent( "CanExecute_UseTags" );
            }
            else {
                InspectionText = item.Name;
            }
        }

        private bool ItemIsTextFile( SourceItem item ) {
            var retValue = false;

            if( item is SourceFile ) {
                var extension = Path.GetExtension( item.FileName );

                retValue = mTextFileExtensions.Any( e => e.Equals( extension ));
            }

            return retValue;
        }

        private bool ItemIsMusicFile( SourceItem item ) {
            var retValue = false;

            if( item is SourceFile ) {
                var extension = Path.GetExtension( item.FileName );

                retValue = mMusicFileExtensions.Any( e => e.Equals( extension ));
            }

            return retValue;
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

        private void ClearTags() {
            mArtistTag = String.Empty;
            mAlbumTag = String.Empty;
            mDateTag = String.Empty;

            RaiseCanExecuteChangedEvent( "CanExecute_UseTags" );
        }

        private string LoadMusicTags( SourceItem item ) {
            var retValue = new StringBuilder();

            retValue.AppendLine( item.FileName );

            try {
                var tags = TagLib.File.Create( item.FileName );

                retValue.AppendLine().AppendLine( "Tags:" ).AppendLine();

                if(!String.IsNullOrWhiteSpace( tags.Tag.JoinedPerformers )) {
                    mArtistTag = string.Join(", ", tags.Tag.JoinedPerformers);

                    retValue.AppendLine( $"Artist: {mArtistTag}" );
                }
                if(!String.IsNullOrWhiteSpace( tags.Tag.Album )) {
                    mAlbumTag = tags.Tag.Album;

                    retValue.AppendLine( $"Album: {mAlbumTag}" );
                }
                if( tags.Tag.Year != 0 ) {
                    mDateTag = tags.Tag.Year.ToString();

                    retValue.AppendLine( $"Date: {mDateTag}" );
                }
                if(!String.IsNullOrWhiteSpace( tags.Tag.Title )) {
                    retValue.AppendLine( $"Title:  {tags.Tag.Title}" );
                }
                if( tags.Tag.Track > 0 ) {
                    retValue.AppendLine( $"Track:  {tags.Tag.Track}" );
                }
            }
            catch( Exception ex ) {
                mLog.LogException( $"LoadMusicTags from: '{item.FileName}'", ex );
            }

            return retValue.ToString();
        }

        public void Execute_TextIsArtist() {
            PublishArtist( SelectedText );
        }

        public bool CanExecute_TextIsArtist() {
            return !string.IsNullOrWhiteSpace( SelectedText );
        }

        private void PublishArtist( string artist ) {
            if(!String.IsNullOrWhiteSpace( artist )) {
                mInspectionChangedSubject.OnNext( new InspectionItemUpdate( InspectionItem.Artist, PathSanitizer.SanitizeFilename( artist.Trim(), ' ' )));
            }
        }

        public void Execute_TextIsAlbum() {
            PublishAlbum( SelectedText );
        }

        public bool CanExecute_TextIsAlbum() {
            return !string.IsNullOrWhiteSpace( SelectedText );
        }

        private void PublishAlbum( string album ) {
            if(!String.IsNullOrWhiteSpace( album )) {
                mInspectionChangedSubject.OnNext( new InspectionItemUpdate( InspectionItem.Album, PathSanitizer.SanitizeFilename( album.Trim(), ' ' )));
            }
        }

        public void Execute_TextIsDate() {
            PublishDate( SelectedText );
        }

        public bool CanExecute_TextIsDate() {
            return !string.IsNullOrWhiteSpace( SelectedText );
        }

        private void PublishDate( string date ) {
            if(!String.IsNullOrWhiteSpace( date )) {
                mInspectionChangedSubject.OnNext( new InspectionItemUpdate( InspectionItem.Date, PathSanitizer.SanitizeFilename( ParseDate( date.Trim()), ' ')));
            }
        }

        public void Execute_UseTags() {
            PublishArtist( mArtistTag );
            PublishAlbum( mAlbumTag );
            PublishDate( mDateTag );
        }

        public bool CanExecute_UseTags() {
            return !String.IsNullOrWhiteSpace( mArtistTag ) || !String.IsNullOrWhiteSpace( mAlbumTag ) || !String.IsNullOrWhiteSpace( mDateTag );
        }

        private string ParseDate( string input ) {
            var retValue = input;

            if( DateTime.TryParse( input, out var result )) {
                retValue = $"{result.Month:D2}-{result.Day:D2}-{result.Year % 100}";
            }

            return retValue;
        }
    }
}
