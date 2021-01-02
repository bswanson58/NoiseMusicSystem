using System;
using System.Reactive.Linq;
using System.Threading;
using Noise.RemoteClient.Dialogs;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace Noise.RemoteClient.ViewModels {
    class TransportViewModel : BindableBase, IDisposable {
        private readonly ITransportProvider         mTransportProvider;
        private readonly ITrackProvider             mTrackProvider;
        private readonly IHostInformationProvider   mHostInformationProvider;
        private readonly IClientManager             mClientManager;
        private readonly IDialogService             mDialogService;
        private readonly IPlatformLog               mLog;
        private IDisposable                         mLibraryStatusSubscription;
        private IDisposable                         mClientStatusSubscription;
        private IDisposable                         mTransportSubscription;
        private bool                                mClientAwake;
        private bool                                mLibraryOpen;
        private string                              mArtistName;
        private string                              mAlbumName;
        private string                              mTrackName;
        private bool                                mIsFavorite;
        private int                                 mRating;
        private double                              mPlayPercentage;
        private TransportInformation                mTrackInformation;

        public  bool                                HasRating => mRating != 0;

        public  TimeSpan                            PlayPosition { get; private set; }
        public  TimeSpan                            TrackLength { get; private set; }
        public  TimeSpan                            TimeRemaining {  get; private set; }
        public  TimeSpan                            TimePlayed {  get; private set; }

        public  DelegateCommand                     EditRatings {  get; }

        public  DelegateCommand                     Play { get; }
        public  DelegateCommand                     Pause { get; }
        public  DelegateCommand                     Stop { get; }
        public  DelegateCommand                     PlayNext { get; }
        public  DelegateCommand                     PlayPrevious { get; }
        public  DelegateCommand                     RepeatTrack { get; }

        public TransportViewModel( ITransportProvider transportProvider, ITrackProvider trackProvider, IHostInformationProvider hostInformationProvider, 
                                   IClientManager clientManager, IPlatformLog log, IDialogService dialogService ) {
            mTransportProvider = transportProvider;
            mTrackProvider = trackProvider;
            mHostInformationProvider = hostInformationProvider;
            mClientManager = clientManager;
            mDialogService = dialogService;
            mLog = log;

            EditRatings = new DelegateCommand( OnEditRatings );
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

        private void OnTransportChanged( TransportInformation status ) {
            mTrackInformation = status;

            ArtistName = status.ArtistName;
            AlbumName = status.AlbumName;
            TrackName = status.TrackName;
            IsFavorite = status.IsFavorite;

            mRating = status.Rating;
            RaisePropertyChanged( nameof( RatingSource ));
            RaisePropertyChanged( nameof( HasRating ));

            TrackLength = TimeSpan.FromTicks( status.TrackLength );
            RaisePropertyChanged( nameof( TrackLength ));

            PlayPosition = TimeSpan.FromTicks( status.PlayPosition );
            RaisePropertyChanged( nameof( PlayPosition ));

            TimePlayed = TimeSpan.FromTicks( status.PlayPosition );
            RaisePropertyChanged( nameof( TimePlayed ));

            TimeRemaining = TimeSpan.FromTicks( status.TrackLength - status.PlayPosition );
            RaisePropertyChanged( nameof( TimeRemaining ));

            PlayPercentage = status.PlayPositionPercentage;
        }

        public string ArtistName {
            get {
                Initialize();

                return mArtistName;
            }
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

        private TrackInfo CreateTrackInfo( TransportInformation fromStatus ) {
            return new TrackInfo {
                TrackId = fromStatus.TrackId, AlbumId = fromStatus.AlbumId, ArtistId = fromStatus.ArtistId,
                TrackName = fromStatus.TrackName, AlbumName = fromStatus.AlbumName, VolumeName = fromStatus.VolumeName, 
                ArtistName = fromStatus.ArtistName,
                TrackNumber = fromStatus.TrackNumber, IsFavorite = fromStatus.IsFavorite, Rating = fromStatus.Rating,
            };
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
                                RaisePropertyChanged( nameof( HasRating ));
                                RaisePropertyChanged( nameof( RatingSource ));
                                IsFavorite = track.IsFavorite;
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
        }
    }
}
