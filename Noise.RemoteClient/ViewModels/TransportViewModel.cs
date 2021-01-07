using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Noise.RemoteClient.Dialogs;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteClient.Models;
using Noise.RemoteClient.Support;
using Noise.RemoteServer.Protocol;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace Noise.RemoteClient.ViewModels {
    class TransportViewModel : BindableBase, IDisposable {
        private readonly ITransportProvider         mTransportProvider;
        private readonly ITrackProvider             mTrackProvider;
        private readonly IHostInformationProvider   mHostInformationProvider;
        private readonly IQueueListener             mQueueListener;
        private readonly IClientManager             mClientManager;
        private readonly IClientState               mClientState;
        private readonly IDialogService             mDialogService;
        private readonly IPreferences               mPreferences;
        private readonly IPlatformLog               mLog;
        private readonly List<TransportTagInfo>     mTags;
        private readonly PlayTimeDisplay            mTimeDisplay;
        private IDisposable                         mLibraryStatusSubscription;
        private IDisposable                         mClientStatusSubscription;
        private IDisposable                         mTransportSubscription;
        private IDisposable                         mNextTrackSubscription;
        private bool                                mClientAwake;
        private bool                                mLibraryOpen;
        private bool                                mPlaybackActive;
        private bool                                mIsPlaying;
        private bool                                mIsPaused;
        private bool                                mIsStopped;
        private bool                                mIsRightTimeDisplayed;
        private bool                                mIsLeftTimeDisplayed;
        private string                              mArtistName;
        private string                              mAlbumName;
        private string                              mTrackName;
        private bool                                mIsFavorite;
        private int                                 mRating;
        private TimeSpan                            mTrackLength;
        private TimeSpan                            mTimePlayed;
        private double                              mPlayPercentage;
        private TransportInformation                mTrackInformation;
        private UiQueuedTrack                       mNextPlayingTrack;
        private int                                 mVolumeLevel;

        public  bool                                HasRating => mRating != 0;
        public  bool                                NeedRating => !HasRating && !IsFavorite;
        public  bool                                HaveTags => mTags.Any();
        public  bool                                NeedTags => !HaveTags;
        public  bool                                HaveNextPlayingTrack => !String.IsNullOrWhiteSpace( mNextPlayingTrack?.TrackName );

        public  bool                                DisplayVolume { get; private set; }

        public  TimeSpan                            PlayPosition { get; private set; }
        public  TimeSpan                            TrackLength { get; private set; }
        public  TimeSpan                            LeftTime {  get; private set; }
        public  TimeSpan                            RightTime {  get; private set; }
        public  string                              Tags => String.Join( " | ", from t in mTags select t.TagName );
        public  string                              NextPlayingTrack { get; private set; }

        public  DelegateCommand                     DisplaySuggestions { get; }
        public  DelegateCommand                     DisplayAlbums { get; }
        public  DelegateCommand                     DisplayTracks { get; }
        public  DelegateCommand                     DisplayNextPlay { get; }

        public  DelegateCommand                     EditRatings {  get; }
        public  DelegateCommand                     EditTags { get; }

        public  DelegateCommand                     ToggleTimeDisplay { get; }
        public  DelegateCommand                     ToggleVolumeDisplay { get; }

        public  DelegateCommand                     Play { get; }
        public  DelegateCommand                     Pause { get; }
        public  DelegateCommand                     Stop { get; }
        public  DelegateCommand                     PlayNext { get; }
        public  DelegateCommand                     PlayPrevious { get; }
        public  DelegateCommand                     RepeatTrack { get; }

        public TransportViewModel( ITransportProvider transportProvider, ITrackProvider trackProvider, IHostInformationProvider hostInformationProvider, 
                                   IClientManager clientManager, IClientState clientState, IQueueListener queueListener,
                                   IPlatformLog log, IDialogService dialogService, IPreferences preferences ) {
            mTransportProvider = transportProvider;
            mTrackProvider = trackProvider;
            mHostInformationProvider = hostInformationProvider;
            mQueueListener = queueListener;
            mClientManager = clientManager;
            mClientState = clientState;
            mDialogService = dialogService;
            mPreferences = preferences;
            mLog = log;

            mTimeDisplay = new PlayTimeDisplay();
            mTimeDisplay.SetMode( mPreferences.Get( PreferenceNames.PlaybackTimeFormat, mTimeDisplay.DefaultMode ));

            mTags = new List<TransportTagInfo>();

            DisplayAlbums = new DelegateCommand( OnDisplayAlbums );
            DisplayTracks = new DelegateCommand( OnDisplayTracks );
            DisplaySuggestions = new DelegateCommand( OnDisplaySuggestions );
            DisplayNextPlay = new DelegateCommand( OnDisplayNextPlay );

            EditRatings = new DelegateCommand( OnEditRatings );
            EditTags = new DelegateCommand( OnEditTags );

            ToggleTimeDisplay = new DelegateCommand( OnToggleTimeDisplay );
            ToggleVolumeDisplay = new DelegateCommand( OnToggleVolumeDisplay );

            Play = new DelegateCommand( OnPlay );
            Pause = new DelegateCommand( OnPause );
            Stop = new DelegateCommand( OnStop );
            PlayPrevious = new DelegateCommand( OnPlayPrevious );
            PlayNext = new DelegateCommand( OnPlayNext );
            RepeatTrack = new DelegateCommand( OnRepeatTrack );
        }

        private void Initialize() {
            if( mLibraryStatusSubscription == null ) {
                mLibraryStatusSubscription = mHostInformationProvider.LibraryStatus.ObserveOn( SynchronizationContext.Current ).Subscribe( OnHostStatus );
                mClientStatusSubscription = mClientManager.ClientStatus.Subscribe( OnClientStatus );
                mNextTrackSubscription = mQueueListener.NextPlayingTrack.Subscribe( OnNextPlayingTrack );
            }
        }

        private void OnClientStatus( ClientStatus status ) {
            mClientAwake = status?.ClientState == eClientState.Starting;

            StartStatus();
        }

        private void OnHostStatus( LibraryStatus status ) {
            mLibraryOpen = status?.LibraryOpen == true;

            StartStatus();
        }

        private void StartStatus() {
            try {
                if(( mLibraryOpen ) &&
                   ( mClientAwake )) {
                    mTransportSubscription = mTransportProvider.TransportStatus.Subscribe( OnTransportChanged );
                    mTransportProvider.StartTransportStatusRequests();
                }
                else {
                    mTransportProvider.StopTransportStatusRequests();

                    mTransportSubscription?.Dispose();
                    mTransportSubscription = null;
                }
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( StartStatus ), ex );
            }
        }

        private void OnNextPlayingTrack( UiQueuedTrack track ) {
            mNextPlayingTrack = track;

            if( HaveNextPlayingTrack ) {
                NextPlayingTrack = $"{mNextPlayingTrack.TrackName}/{mNextPlayingTrack.ArtistName}";
            }

            RaisePropertyChanged( nameof( NextPlayingTrack ));
            RaisePropertyChanged( nameof( HaveNextPlayingTrack ));
        }

        private void OnTransportChanged( TransportInformation status ) {
            mTrackInformation = status;

            ArtistName = status.ArtistName;
            AlbumName = status.AlbumName;
            TrackName = status.TrackName;

            IsFavorite = status.IsFavorite;
            mRating = status.Rating;
            RaisePropertyChanged( nameof( RatingSource ));
            RaisePropertyChanged( nameof( HasRating ));
            RaisePropertyChanged( nameof( NeedRating ));

            TrackLength = TimeSpan.FromTicks( status.TrackLength );
            RaisePropertyChanged( nameof( TrackLength ));

            PlayPosition = TimeSpan.FromTicks( status.PlayPosition );
            RaisePropertyChanged( nameof( PlayPosition ));

            mTrackLength = TimeSpan.FromTicks( status.TrackLength );
            mTimePlayed = TimeSpan.FromTicks( status.PlayPosition );
            DisplayTrackTimes();
            PlayPercentage = status.PlayPositionPercentage;

            IsPaused = mTrackInformation?.TransportState == TransportState.Paused;
            IsPlaying = mTrackInformation?.TransportState == TransportState.Playing;
            IsPlaybackActive = IsPlaying || IsPaused;
            IsStopped = mTrackInformation?.TransportState == TransportState.Stopped || mTrackInformation?.TransportState == TransportState.Unknown;

            mTags.Clear();
            mTags.AddRange( status.Tags );
            RaisePropertyChanged( nameof( Tags ));
            RaisePropertyChanged( nameof( NeedTags ));
            RaisePropertyChanged( nameof( HaveTags ));
        }

        public bool IsPlaybackActive {
            get {
                Initialize();

                return mPlaybackActive;
            }
            set => SetProperty( ref mPlaybackActive, value );
        }

        public bool IsPlaying {
            get => mIsPlaying;
            set => SetProperty( ref mIsPlaying, value );
        }

        public bool IsPaused {
            get => mIsPaused;
            set => SetProperty( ref mIsPaused, value );
        }

        public bool IsStopped {
            get => mIsStopped;
            set => SetProperty( ref mIsStopped, value );
        }

        public string ArtistName {
            get => mArtistName;
            set => SetProperty( ref mArtistName, value );
        }

        public string AlbumName {
            get => mAlbumName;
            set => SetProperty( ref mAlbumName, value );
        }

        public string TrackName {
            get => mTrackName;
            set => SetProperty( ref mTrackName, value );
        }

        public bool IsFavorite {
            get => mIsFavorite;
            set => SetProperty( ref mIsFavorite, value );
        }

        public string RatingSource {
            get {
                var retValue = "0_Star";

                if(( mRating > 0 ) &&
                   ( mRating < 6 )) {
                    retValue = $"{mRating:D1}_Star";
                }

                return retValue;
            }
        }

        public double PlayPercentage {
            get => mPlayPercentage;
            set => SetProperty( ref mPlayPercentage, value );
        }

        private void OnToggleTimeDisplay() {
            mTimeDisplay.SetNextMode();
            mPreferences.Set( PreferenceNames.PlaybackTimeFormat, mTimeDisplay.CurrentMode );

            DisplayTrackTimes();
        }

        private void DisplayTrackTimes() {
            if( mTimeDisplay.IsLeftIncrementing ) {
                LeftTime = mTimePlayed;
            }
            else {
                LeftTime = mTimePlayed - mTrackLength;
            }

            if( mTimeDisplay.IsRightIncrementing ) {
                RightTime = mTimePlayed;
            }
            else {
                RightTime = mTimePlayed - mTrackLength;
            }

            IsRightTimeDisplayed = mTimeDisplay.IsRightDisplayed;
            IsLeftTimeDisplayed = mTimeDisplay.IsLeftDisplayed;

            RaisePropertyChanged( nameof( LeftTime ));
            RaisePropertyChanged( nameof( RightTime ));
        }

        public bool IsRightTimeDisplayed {
            get => mIsRightTimeDisplayed;
            set => SetProperty( ref mIsRightTimeDisplayed, value );
        }

        public bool IsLeftTimeDisplayed {
            get => mIsLeftTimeDisplayed;
            set => SetProperty( ref mIsLeftTimeDisplayed, value );
        }

        public int VolumeLevel {
            get => mVolumeLevel;
            set => SetProperty( ref mVolumeLevel, value, OnVolumeChanged );
        }

        private void OnVolumeChanged() {
            mTransportProvider.SetVolumeLevel( VolumeLevel );
        }

        private async void OnToggleVolumeDisplay() {
            DisplayVolume = !DisplayVolume;

            if( DisplayVolume ) {
                var volume = await mTransportProvider.GetVolumeLevel();

                if( volume.Success ) {
                    VolumeLevel = volume.VolumeLevel;
                }
            }

            RaisePropertyChanged( nameof( DisplayVolume ));
        }

        private TrackInfo CreateTrackInfo( TransportInformation fromStatus ) {
            var retValue = new TrackInfo {
                TrackId = fromStatus.TrackId, AlbumId = fromStatus.AlbumId, ArtistId = fromStatus.ArtistId,
                TrackName = fromStatus.TrackName, AlbumName = fromStatus.AlbumName, VolumeName = fromStatus.VolumeName, 
                ArtistName = fromStatus.ArtistName,
                TrackNumber = fromStatus.TrackNumber, IsFavorite = fromStatus.IsFavorite, Rating = fromStatus.Rating,
            };

            retValue.Tags.AddRange( from t in fromStatus.Tags select new TrackTagInfo{ TagId = t.TagId, TagName = t.TagName });

            return retValue;
        }

        private async void OnDisplayAlbums() {
            if(!String.IsNullOrWhiteSpace( mTrackInformation?.ArtistName )) {
                mClientState.SetCurrentArtist( new ArtistInfo { DbId = mTrackInformation.ArtistId, ArtistName = mTrackInformation.ArtistName } );

                await Shell.Current.GoToAsync( RouteNames.AlbumList );
            }
        }

        private async void OnDisplayTracks() {
            if(!String.IsNullOrWhiteSpace( mTrackInformation?.AlbumName )) {
                mClientState.SetCurrentAlbum( new AlbumInfo { ArtistId = mTrackInformation.ArtistId, ArtistName  = mTrackInformation.ArtistName,
                                                              AlbumId = mTrackInformation.AlbumId, AlbumName = mTrackInformation.AlbumName } );

                await Shell.Current.GoToAsync( RouteNames.TrackList );
            }
        }

        private async void OnDisplaySuggestions() {
            if(!String.IsNullOrWhiteSpace( mTrackInformation?.TrackName )) {
                mClientState.SetSuggestionState( new UiQueuedTrack( mTrackInformation ));

                // route to the shell content page, don't push it on the navigation stack.
                await Shell.Current.GoToAsync( $"///{RouteNames.Suggestions}" );
            }
        }

        private async void OnDisplayNextPlay() {
            if(!String.IsNullOrWhiteSpace( mNextPlayingTrack?.TrackName )) {
                mClientState.SetSuggestionState( mNextPlayingTrack );

                // route to the shell content page, don't push it on the navigation stack.
                await Shell.Current.GoToAsync( $"///{RouteNames.Suggestions}" );
            }
        }

        private void OnEditRatings() {
            if(!String.IsNullOrWhiteSpace( mTrackInformation?.TrackName )) {
                var parameters = new DialogParameters {{ EditTrackRatingsViewModel.cTrackParameter, CreateTrackInfo( mTrackInformation ) }};

                mDialogService.ShowDialog( nameof( EditTrackRatingsView ), parameters, async result => {
                    var accepted = result.Parameters.GetValue<bool>( EditTrackRatingsViewModel.cDialogAccepted );

                    if( accepted ) {
                        var track = parameters.GetValue<TrackInfo>( EditTrackRatingsViewModel.cTrackParameter );

                        if( track != null ) {
                            var callStatus = await mTrackProvider.UpdateTrackRatings( track );

                            if(( callStatus.Success ) &&
                               ( track.TrackId.Equals( mTrackInformation?.TrackId ))) {
                                mRating = track.Rating;
                                IsFavorite = track.IsFavorite;

                                RaisePropertyChanged( nameof( HasRating ));
                                RaisePropertyChanged( nameof( RatingSource ));
                                RaisePropertyChanged( nameof( NeedRating ));
                            }
                        }
                    }
                });
            }
        }

        private void OnEditTags() {
            if(!String.IsNullOrWhiteSpace( mTrackInformation?.TrackName )) {
                var parameters = new DialogParameters {{ EditTrackTagsViewModel.cTrackParameter, CreateTrackInfo( mTrackInformation ) }};

                mDialogService.ShowDialog( nameof( EditTrackTagsView ), parameters, async result => {
                    var accepted = result.Parameters.GetValue<bool>( EditTrackTagsViewModel.cDialogAccepted );

                    if( accepted ) {
                        var track = parameters.GetValue<TrackInfo>( EditTrackTagsViewModel.cTrackParameter );

                        if( track != null ) {
                            var callStatus = await mTrackProvider.UpdateTrackTags( track );

                            if(( callStatus.Success ) &&
                               ( track.TrackId.Equals( mTrackInformation?.TrackId ))) {
                                mTags.Clear();
                                mTags.AddRange( from t in track.Tags select new TransportTagInfo{ TagId = t.TagId, TagName = t.TagName });
                                mTrackInformation?.Tags.Clear();
                                mTrackInformation?.Tags.AddRange( from t in track.Tags select new TransportTagInfo{ TagId = t.TagId, TagName = t.TagName });

                                RaisePropertyChanged( nameof( Tags ));
                                RaisePropertyChanged( nameof( HaveTags ));
                                RaisePropertyChanged( nameof( NeedTags ));
                            }
                        }
                    }
                });
            }
        }

        private void OnPlay() {
            mTransportProvider.Play();
        }

        private void OnPause() {
            mTransportProvider.Pause();
        }

        private void OnStop() {
            mTransportProvider.Stop();
        }

        private void OnPlayNext() {
            mTransportProvider.PlayNext();
        }

        private void OnPlayPrevious() {
            mTransportProvider.PlayPrevious();
        }

        private void OnRepeatTrack() {
            mTransportProvider.ReplayTrack();
        }

        public void Dispose() {
            mLibraryStatusSubscription?.Dispose();
            mLibraryStatusSubscription = null;

            mClientStatusSubscription?.Dispose();
            mClientStatusSubscription = null;

            mNextTrackSubscription?.Dispose();
            mNextTrackSubscription = null;
        }
    }
}
