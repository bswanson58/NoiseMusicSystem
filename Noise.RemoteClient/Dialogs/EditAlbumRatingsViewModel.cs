using Noise.RemoteServer.Protocol;
using Prism.Services.Dialogs;

namespace Noise.RemoteClient.Dialogs {
    class EditAlbumRatingsViewModel : EditTrackRatingsViewModel {
        public  const string        cAlbumParameter = "albumParameter";

        private AlbumInfo           mAlbum;

        public  string              AlbumName => mAlbum?.AlbumName;

        public override void OnDialogOpened( IDialogParameters parameters ) {
            mAlbum = parameters.GetValue<AlbumInfo>( cAlbumParameter );

            if( mAlbum != null ) {
                mIsFavorite = mAlbum.IsFavorite;
                mRating = mAlbum.Rating;

                UpdateRating( mRating );

                RaisePropertyChanged( nameof( FavoriteSource ));
                RaisePropertyChanged( nameof( AlbumName ));
            }
        }

        protected override void OnOk() {
            mAlbum.IsFavorite = mIsFavorite;
            mAlbum.Rating = mRating;

            RaiseRequestClose( new DialogParameters{{ cAlbumParameter, mAlbum }, { cDialogAccepted, true }});
        }
    }
}
