using System;
using Noise.RemoteServer.Protocol;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace Noise.RemoteClient.Dialogs {
    class EditTrackRatingsViewModel : BindableBase, IDialogAware {
        public  const string        cTrackParameter = "trackParameter";
        public  const string        cDialogAccepted = "dialogAccepted";

        private TrackInfo           mTrack;
        private bool                mIsFavorite;
        private int                 mRating;

        public  string              TrackName => mTrack?.TrackName;

        public  string              FavoriteSource => mIsFavorite ? "Favorite" : "NotFavorite";
        public  DelegateCommand     ToggleFavorite { get; }

        public  string              Rating0Source => mRating < 0 ? "Negative_Rating" : "Positive_Rating";
        public  string              Rating1Source => mRating > 0 ? "Full_Star" : "Empty_Star";
        public  string              Rating2Source => mRating > 1 ? "Full_Star" : "Empty_Star";
        public  string              Rating3Source => mRating > 2 ? "Full_Star" : "Empty_Star";
        public  string              Rating4Source => mRating > 3 ? "Full_Star" : "Empty_Star";
        public  string              Rating5Source => mRating > 4 ? "Full_Star" : "Empty_Star";
        public  DelegateCommand     Set0Star { get; }
        public  DelegateCommand     Set1Star { get; }
        public  DelegateCommand     Set2Star { get; }
        public  DelegateCommand     Set3Star { get; }
        public  DelegateCommand     Set4Star { get; }
        public  DelegateCommand     Set5Star { get; }
        public  DelegateCommand     ClearRating { get; }

        public  DelegateCommand     Ok { get; }
        public  DelegateCommand     Cancel { get; }

        public  event Action<IDialogParameters> RequestClose;

        public EditTrackRatingsViewModel() {
            ToggleFavorite = new DelegateCommand( OnToggleFavorite );

            Set0Star = new DelegateCommand( OnSetRating0 );
            Set1Star = new DelegateCommand( OnSetRating1 );
            Set2Star = new DelegateCommand( OnSetRating2 );
            Set3Star = new DelegateCommand( OnSetRating3 );
            Set4Star = new DelegateCommand( OnSetRating4 );
            Set5Star = new DelegateCommand( OnSetRating5 );
            ClearRating = new DelegateCommand( OnClearRating );

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
            mTrack = parameters.GetValue<TrackInfo>( cTrackParameter );

            if( mTrack != null ) {
                mIsFavorite = mTrack.IsFavorite;
                mRating = mTrack.Rating;

                UpdateRating( mRating );

                RaisePropertyChanged( nameof( FavoriteSource ));
                RaisePropertyChanged( nameof( TrackName ));
            }
        }

        private void OnToggleFavorite() {
            mIsFavorite = !mIsFavorite;

            RaisePropertyChanged( nameof( FavoriteSource ));
        }

        private void OnSetRating0() => UpdateRating( -1 );
        private void OnSetRating1() => UpdateRating( 1 );
        private void OnSetRating2() => UpdateRating( 2 );
        private void OnSetRating3() => UpdateRating( 3 );
        private void OnSetRating4() => UpdateRating( 4 );
        private void OnSetRating5() => UpdateRating( 5 );
        private void OnClearRating() => UpdateRating( 0 );

        private void UpdateRating( int rating ) {
            mRating = rating;

            RaisePropertyChanged( nameof( Rating0Source ));
            RaisePropertyChanged( nameof( Rating1Source ));
            RaisePropertyChanged( nameof( Rating2Source ));
            RaisePropertyChanged( nameof( Rating3Source ));
            RaisePropertyChanged( nameof( Rating4Source ));
            RaisePropertyChanged( nameof( Rating5Source ));
        }

        private void OnOk() {
            mTrack.IsFavorite = mIsFavorite;
            mTrack.Rating = mRating;

            RaiseRequestClose( new DialogParameters{{ cTrackParameter, mTrack }, { cDialogAccepted, true }});
        }

        private void OnCancel() {
            RaiseRequestClose( new DialogParameters{{ cDialogAccepted, false }});
        }

        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }

        private void RaiseRequestClose( IDialogParameters parameters ) {
            RequestClose?.Invoke( parameters );
        }
    }
}
