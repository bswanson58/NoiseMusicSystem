﻿using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using DynamicData;
using DynamicData.Binding;
using Noise.RemoteClient.Dialogs;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteClient.Support;
using Noise.RemoteServer.Protocol;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Xamarin.Forms;

namespace Noise.RemoteClient.ViewModels {
    class QueueViewModel : BindableBase, IDisposable {
        private readonly IQueueListProvider         mQueueListProvider;
        private readonly ITransportProvider         mTransportProvider;
        private readonly ITrackProvider             mTrackProvider;
        private readonly IClientState               mClientState;
        private readonly IQueueListener             mQueueListener;
        private readonly IPlatformLog               mLog;
        private readonly IDialogService             mDialogService;
        private IDisposable                         mQueueSubscription;
        private IDisposable                         mQueueListSubscription;

        private ObservableCollectionExtended<UiQueuedTrack> mQueueList;

        public  TimeSpan                            TotalTime { get; private set; }
        public  TimeSpan                            RemainingTime { get; private set; }

        public  DelegateCommand<UiQueuedTrack>      Suggestions { get; }
        public  DelegateCommand<UiQueuedTrack>      EditTrackRatings { get; }
        public  DelegateCommand<UiQueuedTrack>      EditTrackTags { get; }

        public  DelegateCommand                     Play { get; }
        public  DelegateCommand                     Pause { get; }
        public  DelegateCommand                     Stop { get; }
        public  DelegateCommand                     PlayNext { get; }
        public  DelegateCommand                     PlayPrevious { get; }
        public  DelegateCommand                     RepeatTrack { get; }
        public  DelegateCommand                     ClearQueue { get; }
        public  DelegateCommand                     ClearPlayedTracks { get; }

        public  DelegateCommand<UiQueuedTrack>      PlayFromTrack { get; }
        public  DelegateCommand<UiQueuedTrack>      ReplayTrack { get; }
        public  DelegateCommand<UiQueuedTrack>      SkipTrack { get; }
        public  DelegateCommand<UiQueuedTrack>      RemoveTrack { get; }
        public  DelegateCommand<UiQueuedTrack>      PromoteTrack { get; }

        public QueueViewModel( IQueueListProvider queueListProvider, ITransportProvider transportProvider, ITrackProvider trackProvider, IQueueListener queueListener,
                               IClientState clientState, IPlatformLog log, IDialogService dialogService ) {
            mQueueListProvider = queueListProvider;
            mQueueListener = queueListener;
            mTransportProvider = transportProvider;
            mTrackProvider = trackProvider;
            mClientState = clientState;
            mDialogService = dialogService;
            mLog = log;

            Suggestions = new DelegateCommand<UiQueuedTrack>( OnSuggestions );
            EditTrackRatings = new DelegateCommand<UiQueuedTrack>( OnEditTrackRatings );
            EditTrackTags = new DelegateCommand<UiQueuedTrack>( OnEditTrackTags );

            Play = new DelegateCommand( OnPlay );
            Pause = new DelegateCommand( OnPause );
            Stop = new DelegateCommand( OnStop );
            PlayPrevious = new DelegateCommand( OnPlayPrevious );
            PlayNext = new DelegateCommand( OnPlayNext );
            RepeatTrack = new DelegateCommand( OnRepeatTrack );
            ClearQueue = new DelegateCommand( OnClearQueue );
            ClearPlayedTracks = new DelegateCommand( OnClearPlayedTracks );

            ReplayTrack = new DelegateCommand<UiQueuedTrack>( OnReplayTrack );
            SkipTrack = new DelegateCommand<UiQueuedTrack>( OnSkipTrack );
            RemoveTrack = new DelegateCommand<UiQueuedTrack>( OnRemoveTrack );
            PromoteTrack = new DelegateCommand<UiQueuedTrack>( OnPromoteTrack );
            PlayFromTrack = new DelegateCommand<UiQueuedTrack>( OnPlayFromTrack );
        }

        public ObservableCollectionExtended<UiQueuedTrack> QueueList {
            get {
                if( mQueueList == null ) {
                    mQueueList = new ObservableCollectionExtended<UiQueuedTrack>();

                    Initialize();
                }

                return mQueueList;
            }
        }

        private void Initialize() {
            mQueueList.CollectionChanged += OnQueueChanged;
            mQueueListSubscription = mQueueListener.QueueList.Connect().ObserveOn( SynchronizationContext.Current ).Bind( mQueueList ).Subscribe();
        }

        private void OnQueueChanged( object sender, NotifyCollectionChangedEventArgs args ) {
            try {
                Device.BeginInvokeOnMainThread(() => {
                    TotalTime = mQueueListener.TotalPlayingTime;
                    RemainingTime = mQueueListener.RemainingPlayTime;

                    RaisePropertyChanged( nameof( TotalTime ));
                    RaisePropertyChanged( nameof( RemainingTime ));
                });
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( OnQueueChanged ), ex );
            }
        }

        private async void OnSuggestions( UiQueuedTrack forTrack ) {
            mClientState.SetSuggestionState( forTrack );

            // route to the shell content page, don't push it on the navigation stack.
            await Shell.Current.GoToAsync( $"///{RouteNames.Suggestions}" );
        }

        private TrackInfo CreateTrackInfo( UiQueuedTrack fromQueuedTrack ) {
            var retValue = new TrackInfo {
                TrackId = fromQueuedTrack.Track.TrackId, AlbumId = fromQueuedTrack.Track.AlbumId, ArtistId = fromQueuedTrack.Track.ArtistId,
                TrackName = fromQueuedTrack.TrackName, AlbumName = fromQueuedTrack.AlbumName, VolumeName = fromQueuedTrack.Track.VolumeName, 
                ArtistName = fromQueuedTrack.ArtistName, Duration = fromQueuedTrack.Track.Duration,
                TrackNumber = fromQueuedTrack.Track.TrackNumber, IsFavorite = fromQueuedTrack.IsFavorite, Rating = fromQueuedTrack.Rating,
            };

            retValue.Tags.AddRange( from t in fromQueuedTrack.Track.Tags select new TrackTagInfo{ TagId = t.TagId, TagName = t.TagName });

            return retValue;
        }

        private void OnEditTrackRatings( UiQueuedTrack forTrack ) {
            var parameters = new DialogParameters {{ EditTrackRatingsViewModel.cTrackParameter, CreateTrackInfo( forTrack ) }};

            mDialogService.ShowDialog( nameof( EditTrackRatingsView ), parameters, async result => {
                var accepted = result.Parameters.GetValue<bool>( EditTrackRatingsViewModel.cDialogAccepted );

                if( accepted ) {
                    var track = parameters.GetValue<TrackInfo>( EditTrackRatingsViewModel.cTrackParameter );

                    if( track != null ) {
                        await mTrackProvider.UpdateTrackRatings( track );

                        var queueTrack = QueueList.FirstOrDefault( t => t.Track.TrackId.Equals( track.TrackId ));
                        queueTrack?.UpdateRatings( track );
                    }
                }
            });
        }

        private void OnEditTrackTags( UiQueuedTrack forTrack ) {
            var parameters = new DialogParameters {{ EditTrackTagsViewModel.cTrackParameter, CreateTrackInfo( forTrack ) }};

            mDialogService.ShowDialog( nameof( EditTrackTagsView ), parameters, async result => {
                var accepted = result.Parameters.GetValue<bool>( EditTrackTagsViewModel.cDialogAccepted );

                if( accepted ) {
                    var track = parameters.GetValue<TrackInfo>( EditTrackTagsViewModel.cTrackParameter );

                    if( track != null ) {
                        await mTrackProvider.UpdateTrackTags( track );

                        var queueTrack = QueueList.FirstOrDefault( t => t.Track.TrackId.Equals( track.TrackId ));
                        queueTrack?.UpdateTags( track );
                    }
                }
            });
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

        private void OnClearQueue() {
            mQueueListProvider.ClearQueue();
        }

        private void OnClearPlayedTracks() {
            mQueueListProvider.ClearPlayedTracks();
        }

        private void OnReplayTrack( UiQueuedTrack track ) {
            mQueueListProvider.ReplayQueueItem( track.Track );
        }

        private void OnSkipTrack( UiQueuedTrack track ) {
            mQueueListProvider.SkipQueueItem( track.Track );
        }

        private void OnPromoteTrack( UiQueuedTrack track ) {
            mQueueListProvider.PromoteQueueItem( track.Track );
        }

        private void OnRemoveTrack( UiQueuedTrack track ) {
            mQueueListProvider.RemoveQueueItem( track.Track );
        }

        private void OnPlayFromTrack( UiQueuedTrack track ) {
            mQueueListProvider.PlayFromQueueItem( track.Track );
        }

        public void Dispose() {
            mQueueSubscription?.Dispose();
            mQueueSubscription = null;

            mQueueListSubscription?.Dispose();
            mQueueListSubscription = null;
        }
    }
}
