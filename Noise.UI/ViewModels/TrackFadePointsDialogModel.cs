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
        }

        public void Execute_SetFadeInTime() {
            FadeInTime = CurrentTime;

            RaisePropertyChanged( () => FadeInTime );
        }

        public void Execute_SetFadeOutTime() {
            FadeOutTime = CurrentTime;

            RaisePropertyChanged( () => FadeOutTime );
        }

        public void SaveFadePoints() {
            if( TimeSpan.TryParse( FadeInTime, CultureInfo.CurrentUICulture, out TimeSpan timeIn )) {
                mFadePoints.FadeInPoint = timeIn.Seconds;
            }
            if( TimeSpan.TryParse( FadeOutTime, CultureInfo.CurrentUICulture, out TimeSpan timeOut )) {
                mFadePoints.FadeOutPoint = timeOut.Seconds;
            }

            mContextWriter.SaveTrackFadePoints( mTrack, mFadePoints );
        }
    }
}
