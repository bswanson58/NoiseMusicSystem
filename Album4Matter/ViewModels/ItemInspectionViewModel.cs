using System;
using System.Reactive.Subjects;
using Album4Matter.Dto;
using Album4Matter.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Album4Matter.ViewModels {
    class ItemInspectionViewModel : AutomaticCommandBase, IItemInspectionViewModel {
        private readonly Subject<InspectionItemUpdate>  mInspectionChangedSubject;
        private string                                  mSelectedText;

        public IObservable<InspectionItemUpdate>        InspectionItemChanged => mInspectionChangedSubject;

        public  string                                  InspectionItemName { get; private set; }
        public  string                                  InspectionText { get; set; }

        public ItemInspectionViewModel() {
            mInspectionChangedSubject = new Subject<InspectionItemUpdate>();
        }

        public void SetInspectionItem( SourceItem item ) {
            InspectionItemName = item.Name;
            InspectionText = item.Name;

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

        public void Execute_TextIsArtist() { }

        public bool CanExecute_TextIsArtist() {
            return !string.IsNullOrWhiteSpace( SelectedText );
        }

        public void Execute_TextIsAlbum() { }

        public bool CanExecute_TextIsAlbum() {
            return !string.IsNullOrWhiteSpace( SelectedText );
        }

        public void Execute_TextIsDate() { }

        public bool CanExecute_TextIsDate() {
            return !string.IsNullOrWhiteSpace( SelectedText );
        }
    }
}
