using System;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using Album4Matter.Dto;
using Album4Matter.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Album4Matter.ViewModels {
    class ItemInspectionViewModel : AutomaticCommandBase, IItemInspectionViewModel {
        private readonly string[]                       mTextFileExtensions = { ".txt", ".nfo" };

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
            if( ItemIsTextFile( item )) {
                InspectionText = LoadTextFile( item.FileName );
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

        public void Execute_TextIsArtist() {
            mInspectionChangedSubject.OnNext( new InspectionItemUpdate( InspectionItem.Artist, SelectedText.Trim()));
        }

        public bool CanExecute_TextIsArtist() {
            return !string.IsNullOrWhiteSpace( SelectedText );
        }

        public void Execute_TextIsAlbum() {
            mInspectionChangedSubject.OnNext( new InspectionItemUpdate( InspectionItem.Album, SelectedText.Trim()));
        }

        public bool CanExecute_TextIsAlbum() {
            return !string.IsNullOrWhiteSpace( SelectedText );
        }

        public void Execute_TextIsDate() {
            mInspectionChangedSubject.OnNext( new InspectionItemUpdate( InspectionItem.Date, SelectedText.Trim()));
        }

        public bool CanExecute_TextIsDate() {
            return !string.IsNullOrWhiteSpace( SelectedText );
        }
    }
}
