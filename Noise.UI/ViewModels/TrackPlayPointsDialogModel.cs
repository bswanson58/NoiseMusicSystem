using System;
using System.Globalization;
using System.Threading.Tasks;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
    public class TrackPlayPointsDialogModel : PropertyChangeBase, IDialogAware {
        public  const string                    cTrackParameter = "track";
        public  const string                    cCurrentPosition = "position";
        public  const string                    cTrackLength = "trackLength";

        private readonly IPlaybackContextWriter mContextWriter;
        private readonly INoiseLog              mLog;
        private DbTrack                         mTrack;
        private TimeSpan                        mCurrentPosition;
        private TimeSpan                        mTrackLength;
        private ScTrackPlayPoints               mPlayPoints;

        public  string                          CurrentTime {  get; set; }
        public  string                          PlayStartTime { get; private set; }
        public  string                          PlayStopTime { get; private set; }

        public  DelegateCommand                 Ok { get; }
        public  DelegateCommand                 Cancel { get; }
        public  DelegateCommand                 SetEndTime { get; }
        public  DelegateCommand                 ClearEndTime { get; }
        public  DelegateCommand                 SetStartTime { get; }
        public  DelegateCommand                 ClearStartTime { get; }

        public  string                          Title { get; }
        public  event Action<IDialogResult>     RequestClose;

        public TrackPlayPointsDialogModel( IPlaybackContextWriter playbackContextWriter, INoiseLog log ) {
            mContextWriter = playbackContextWriter;
            mLog = log;

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );
            SetEndTime = new DelegateCommand( OnSetEndTime, CanSetEndTime );
            ClearEndTime = new DelegateCommand( OnClearEndTime, CanClearEndTime );
            SetStartTime = new DelegateCommand( OnSetStartTime, CanSetStartTime );
            ClearStartTime = new DelegateCommand( OnClearStartTime, CanClearStartTime );

            Title = "Track Play Points";
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
            mTrack = parameters.GetValue<DbTrack>( cTrackParameter );
            mCurrentPosition = TimeSpan.FromTicks( parameters.GetValue<long>( cCurrentPosition ));
            mTrackLength = TimeSpan.FromTicks( parameters.GetValue<long>( cTrackLength ));

            mPlayPoints = mContextWriter.GetTrackPlayPoints( mTrack );

            CurrentTime = mCurrentPosition.ToString();

            if( mPlayPoints != null ) {
                PlayStartTime = TimeSpan.FromSeconds( mPlayPoints.StartPlaySeconds ).ToString();
                PlayStopTime = TimeSpan.FromSeconds( mPlayPoints.StopPlaySeconds ).ToString();
            }

            UpdateControls();
        }

        private void OnSetStartTime() {
            PlayStartTime = CurrentTime;

            UpdateControls();
        }

        private bool CanSetStartTime() {
            var retValue = false;
            
            if( TimeSpan.TryParse( PlayStopTime, CultureInfo.CurrentUICulture, out TimeSpan timeOut )) {
                if(((int)timeOut.TotalSeconds == 0 ) ||
                   ( mCurrentPosition < timeOut )) {
                    retValue = true;
                }
            }

            return retValue;
        }

        private void OnClearStartTime() {
            PlayStartTime = TimeSpan.FromSeconds( 0 ).ToString();

            UpdateControls();
        }

        private bool CanClearStartTime() {
            var retValue = false;
            
            if( TimeSpan.TryParse( PlayStartTime, CultureInfo.CurrentUICulture, out TimeSpan timeIn )) {
                retValue = (int)timeIn.TotalSeconds != 0;
            }

            return retValue;
        }

        private void OnSetEndTime() {
            PlayStopTime = CurrentTime;

            UpdateControls();
        }

        private bool CanSetEndTime() {
            var retValue = false;
            
            if( TimeSpan.TryParse( PlayStartTime, CultureInfo.CurrentUICulture, out TimeSpan timeIn )) {
                if(((int)timeIn.TotalSeconds == 0 ) ||
                   ( mCurrentPosition > timeIn )) {
                    retValue = true;
                }
            }

            return retValue;
        }

        private void OnClearEndTime() {
            PlayStopTime = TimeSpan.FromSeconds( 0 ).ToString();

            UpdateControls();
        }

        private bool CanClearEndTime() {
            var retValue = false;
            
            if( TimeSpan.TryParse( PlayStopTime, CultureInfo.CurrentUICulture, out TimeSpan timeOut )) {
                retValue = (int)timeOut.TotalSeconds != 0;
            }

            return retValue;
        }

        public async void SavePlayPoints() {
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
            if( mPlayPoints.StartPlaySeconds >= ( mPlayPoints.StopPlaySeconds > 0 ? mPlayPoints.StopPlaySeconds : mTrackLength.TotalSeconds )) {
                mPlayPoints.StartPlaySeconds = 0;
            }

            await Task.Run( () => {
                try {
                    mContextWriter.SaveTrackPlayPoints( mTrack, mPlayPoints );
                }
                catch( Exception ex ) {
                    mLog.LogException( "TrackPlayPointsDialogModel:SavePlayPoints", ex );
                }
            });
        }

        private void UpdateControls() {
            RaisePropertyChanged( () => PlayStartTime );
            RaisePropertyChanged( () => PlayStopTime );
            RaisePropertyChanged( () => CurrentTime );
            ClearStartTime.RaiseCanExecuteChanged();
            ClearEndTime.RaiseCanExecuteChanged();
            SetStartTime.RaiseCanExecuteChanged();
            SetEndTime.RaiseCanExecuteChanged();
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        public void OnOk() {
            SavePlayPoints();

            RaiseRequestClose( new DialogResult( ButtonResult.OK ));
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
    }
}

