using System;
using Noise.UI.Dto;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
    class ArtistEditDialogModel : PropertyChangeBase, IDialogAware {
        public  const string                        cArtistParameter = "artist";

        public  UiArtist                            Artist { get; private set; }
        public  string                              Title { get; }

        public  DelegateCommand                     Ok { get; }
        public  DelegateCommand                     Cancel { get; }
        public  event Action<IDialogResult>         RequestClose;

        public ArtistEditDialogModel() {
            Title = "Artist Properties";

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
            Artist = parameters.GetValue<UiArtist>( cArtistParameter );

            RaisePropertyChanged( () => Artist );
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        public void OnOk() {
            var parameters = new DialogParameters{{ cArtistParameter, Artist }};

            RaiseRequestClose( new DialogResult( ButtonResult.OK, parameters ));
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
    }
}
