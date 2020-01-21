using System;
using System.Globalization;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
    public class TrackPlayPointsDialogModel : DialogModelBase {
        private readonly IPlaybackContextWriter mContextWriter;
        private DbTrack                         mTrack;
        private TimeSpan                        mCurrentPosition;
        private TimeSpan                        mTrackLength;
        private ScTrackPlayPoints               mPlayPoints;

        public  string                          CurrentTime {  get; set; }
        public  string                          PlayStartTime { get; private set; }
        public  string                          PlayStopTime { get; private set; }

        public TrackPlayPointsDialogModel( IPlaybackContextWriter playbackContextWriter ) {
            mContextWriter = playbackContextWriter;
        }

        public void SetTrack( DbTrack track, long currentPosition, long trackLength ) {
            mTrack = track;
            mCurrentPosition = TimeSpan.FromTicks( currentPosition );
            mTrackLength = TimeSpan.FromTicks( trackLength );

            mPlayPoints = mContextWriter.GetTrackPlayPoints( mTrack );

            CurrentTime = mCurrentPosition.ToString();

            if( mPlayPoints != null ) {
                PlayStartTime = TimeSpan.FromSeconds( mPlayPoints.StartPlaySeconds ).ToString();
                PlayStopTime = TimeSpan.FromSeconds( mPlayPoints.StopPlaySeconds ).ToString();
            }

            UpdateControls();
        }

        public void Execute_SetStartTime() {
            PlayStartTime = CurrentTime;

            UpdateControls();
        }

        public bool CanExecute_SetStartTime() {
            var retValue = false;
            
            if( TimeSpan.TryParse( PlayStopTime, CultureInfo.CurrentUICulture, out TimeSpan timeOut )) {
                if(((int)timeOut.TotalSeconds == 0 ) ||
                   ( mCurrentPosition < timeOut )) {
                    retValue = true;
                }
            }

            return retValue;
        }

        public void Execute_ClearStartTime() {
            PlayStartTime = TimeSpan.FromSeconds( 0 ).ToString();

            UpdateControls();
        }

        public bool CanExecute_ClearStartTime() {
            var retValue = false;
            
            if( TimeSpan.TryParse( PlayStartTime, CultureInfo.CurrentUICulture, out TimeSpan timeIn )) {
                retValue = (int)timeIn.TotalSeconds != 0;
            }

            return retValue;
        }

        public void Execute_SetEndTime() {
            PlayStopTime = CurrentTime;

            UpdateControls();
        }

        public bool CanExecute_SetEndTime() {
            var retValue = false;
            
            if( TimeSpan.TryParse( PlayStartTime, CultureInfo.CurrentUICulture, out TimeSpan timeIn )) {
                if(((int)timeIn.TotalSeconds == 0 ) ||
                   ( mCurrentPosition > timeIn )) {
                    retValue = true;
                }
            }

            return retValue;
        }

        public void Execute_ClearEndTime() {
            PlayStopTime = TimeSpan.FromSeconds( 0 ).ToString();

            UpdateControls();
        }

        public bool CanExecute_ClearEndTime() {
            var retValue = false;
            
            if( TimeSpan.TryParse( PlayStopTime, CultureInfo.CurrentUICulture, out TimeSpan timeOut )) {
                retValue = (int)timeOut.TotalSeconds != 0;
            }

            return retValue;
        }

        public void SavePlayPoints() {
            if( TimeSpan.TryParse( PlayStartTime, CultureInfo.CurrentUICulture, out TimeSpan timeIn )) {
                if( timeIn < mTrackLength ) {
                    mPlayPoints.StartPlaySeconds = (long)timeIn.TotalSeconds;
                }
                else {
                    mPlayPoints.StartPlaySeconds = 0;
                }
            }
            if( TimeSpan.TryParse( PlayStopTime, CultureInfo.CurrentUICulture, out TimeSpan timeOut )) {
                if( timeOut < mTrackLength ) {
                    mPlayPoints.StopPlaySeconds = (long)timeOut.TotalSeconds;
                }
                else {
                    mPlayPoints.StopPlaySeconds = 0;
                }
            }
            if( mPlayPoints.StartPlaySeconds >= Math.Min( mPlayPoints.StopPlaySeconds, mTrackLength.TotalSeconds )) {
                mPlayPoints.StartPlaySeconds = 0;
            }

            mContextWriter.SaveTrackPlayPoints( mTrack, mPlayPoints );
        }

        private void UpdateControls() {
            RaisePropertyChanged( () => PlayStartTime );
            RaisePropertyChanged( () => PlayStopTime );
            RaisePropertyChanged( () => CurrentTime );
            RaiseCanExecuteChangedEvent( nameof( CanExecute_ClearStartTime ));
            RaiseCanExecuteChangedEvent( nameof( CanExecute_ClearEndTime ));
            RaiseCanExecuteChangedEvent( nameof( CanExecute_SetStartTime ));
            RaiseCanExecuteChangedEvent( nameof( CanExecute_SetEndTime ));
        }
    }
}
