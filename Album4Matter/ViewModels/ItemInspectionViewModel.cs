using ReusableBits.Mvvm.ViewModelSupport;

namespace Album4Matter.ViewModels {
    class ItemInspectionViewModel : AutomaticCommandBase {
        private string      mSelectedText;

        public  string      InspectionItemName { get; private set; }
        public  string      InspectionText { get; set; }

        public ItemInspectionViewModel() {

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
