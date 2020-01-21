using System;
using System.Globalization;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
    public class TrackFadePointsDialogModel : DialogModelBase {
        private readonly IPlaybackContextWriter mContextWriter;
        private DbTrack                         mTrack;
        private TimeSpan                        mCurrentPosition;
        private TimeSpan                        mTrackLength;
        private ScTrackFadePoints               mFadePoints;

        public  string                          CurrentTime {  get; set; }
        public  string                          FadeInTime { get; private set; }
        public  string                          FadeOutTime { get; private set; }

        public TrackFadePointsDialogModel( IPlaybackContextWriter playbackContextWriter ) {
            mContextWriter = playbackContextWriter;
        }

        public void SetTrack( DbTrack track, long currentPosition, long trackLength ) {
            mTrack = track;
            mCurrentPosition = TimeSpan.FromTicks( currentPosition );
            mTrackLength = TimeSpan.FromTicks( trackLength );

            mFadePoints = mContextWriter.GetTrackFadePoints( mTrack );

            CurrentTime = mCurrentPosition.ToString();

            if( mFadePoints != null ) {
                FadeInTime = TimeSpan.FromSeconds( mFadePoints.FadeInPoint ).ToString();
                FadeOutTime = TimeSpan.FromSeconds( mFadePoints.FadeOutPoint ).ToString();
            }

            UpdateControls();
        }

        public void Execute_SetFadeInTime() {
            FadeInTime = CurrentTime;

            UpdateControls();
        }

        public bool CanExecute_SetFadeInTime() {
            var retValue = false;
            
            if( TimeSpan.TryParse( FadeOutTime, CultureInfo.CurrentUICulture, out TimeSpan timeOut )) {
                if(((int)timeOut.TotalSeconds == 0 ) ||
                   ( mCurrentPosition < timeOut )) {
                    retValue = true;
                }
            }

            return retValue;
        }

        public void Execute_ClearFadeInTime() {
            FadeInTime = TimeSpan.FromSeconds( 0 ).ToString();

            UpdateControls();
        }

        public bool CanExecute_ClearFadeInTime() {
            var retValue = false;
            
            if( TimeSpan.TryParse( FadeInTime, CultureInfo.CurrentUICulture, out TimeSpan timeIn )) {
                retValue = (int)timeIn.TotalSeconds != 0;
            }

            return retValue;
        }

        public void Execute_SetFadeOutTime() {
            FadeOutTime = CurrentTime;

            UpdateControls();
        }

        public bool CanExecute_SetFadeOutTime() {
            var retValue = false;
            
            if( TimeSpan.TryParse( FadeInTime, CultureInfo.CurrentUICulture, out TimeSpan timeIn )) {
                if(((int)timeIn.TotalSeconds == 0 ) ||
                   ( mCurrentPosition > timeIn )) {
                    retValue = true;
                }
            }

            return retValue;
        }

        public void Execute_ClearFadeOutTime() {
            FadeOutTime = TimeSpan.FromSeconds( 0 ).ToString();

            UpdateControls();
        }

        public bool CanExecute_ClearFadeOutTime() {
            var retValue = false;
            
            if( TimeSpan.TryParse( FadeOutTime, CultureInfo.CurrentUICulture, out TimeSpan timeOut )) {
                retValue = (int)timeOut.TotalSeconds != 0;
            }

            return retValue;
        }

        public void SaveFadePoints() {
            if( TimeSpan.TryParse( FadeInTime, CultureInfo.CurrentUICulture, out TimeSpan timeIn )) {
                if( timeIn < mTrackLength ) {
                    mFadePoints.FadeInPoint = (long)timeIn.TotalSeconds;
                }
                else {
                    mFadePoints.FadeInPoint = 0;
                }
            }
            if( TimeSpan.TryParse( FadeOutTime, CultureInfo.CurrentUICulture, out TimeSpan timeOut )) {
                if( timeOut < mTrackLength ) {
                    mFadePoints.FadeOutPoint = (long)timeOut.TotalSeconds;
                }
                else {
                    mFadePoints.FadeOutPoint = 0;
                }
            }
            if( mFadePoints.FadeInPoint >= Math.Min( mFadePoints.FadeOutPoint, mTrackLength.TotalSeconds )) {
                mFadePoints.FadeInPoint = 0;
            }

            mContextWriter.SaveTrackFadePoints( mTrack, mFadePoints );
        }

        private void UpdateControls() {
            RaisePropertyChanged( () => FadeInTime );
            RaisePropertyChanged( () => FadeOutTime );
            RaisePropertyChanged( () => CurrentTime );
            RaiseCanExecuteChangedEvent( "CanExecute_SetFadeInTime" );
            RaiseCanExecuteChangedEvent( "CanExecute_SetFadeOutTime" );
            RaiseCanExecuteChangedEvent( "CanExecute_ClearFadeInTime" );
            RaiseCanExecuteChangedEvent( "CanExecute_ClearFadeOutTime" );
        }
    }
}
