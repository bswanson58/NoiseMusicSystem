﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using DynamicData;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;
using Polly;

namespace Noise.RemoteClient.Models {
    class QueueListener : IQueueListener, IDisposable {
        private readonly IHostInformationProvider       mHostInformationProvider;
        private readonly IClientManager                 mClientManager;
        private readonly IQueueListProvider             mQueueListProvider;
        private readonly IPlatformLog                   mLog;
        private readonly SourceList<UiQueuedTrack>      mQueueList;
        private readonly BehaviorSubject<PlayingState>  mPlayingState;
        private readonly BehaviorSubject<UiQueuedTrack> mNextPlayingTrack;
        private IDisposable                             mLibraryStatusSubscription;
        private IDisposable                             mClientStatusSubscription;
        private IDisposable                             mQueueSubscription;
        private bool                                    mClientAwake;
        private bool                                    mLibraryOpen;

        public  IObservableList<UiQueuedTrack>          QueueList => mQueueList.AsObservableList();
        public  IObservable<UiQueuedTrack>              NextPlayingTrack => mNextPlayingTrack;
        public  IObservable<PlayingState>               CurrentlyPlaying => mPlayingState;

        public  TimeSpan                                TotalPlayingTime { get; private set; }
        public  TimeSpan                                RemainingPlayTime { get; private set; }

        public QueueListener( IQueueListProvider queueListProvider, IClientManager clientManager,
                              IHostInformationProvider hostInformationProvider, IPlatformLog log ) {
            mHostInformationProvider = hostInformationProvider;
            mQueueListProvider = queueListProvider;
            mClientManager = clientManager;
            mLog = log;

            mQueueList = new SourceList<UiQueuedTrack>();
            mPlayingState = new BehaviorSubject<PlayingState>( new PlayingState());
            mNextPlayingTrack = new BehaviorSubject<UiQueuedTrack>( new UiQueuedTrack());

            Initialize();
        }

        private void Initialize() {
            mLibraryStatusSubscription = mHostInformationProvider.LibraryStatus.Subscribe( OnHostStatus );
            mClientStatusSubscription = mClientManager.ClientStatus.Subscribe( OnClientStatus );
        }

        private void OnClientStatus( ClientStatus status ) {
            mClientAwake = status?.ClientState == eClientState.Starting;

            StartQueue();
        }

        private void OnHostStatus( LibraryStatus status ) {
            mLibraryOpen = status?.LibraryOpen == true;

            StartQueue();
        }

        private void StartQueue() {
            try {
                if(( mLibraryOpen ) &&
                   ( mClientAwake )) {
                    mQueueSubscription = mQueueListProvider.QueueListStatus.Subscribe( OnQueueChanged );

                    StartQueueStatus();
                }
                else {
                    mQueueListProvider.StopQueueStatusRequests();

                    mQueueSubscription?.Dispose();
                    mQueueSubscription = null;

                    mQueueList.Clear();
                }
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( StartQueue ), ex );
            }
        }

        private void StartQueueStatus() {
            // If the request returns with a false result, restart it again (up to 10 times).
            // Cancelling the status requests should return a true result, and failures returns a false.
            Policy
                .HandleResult( false )
                .WaitAndRetryAsync( 10, retryAfter => TimeSpan.FromSeconds( 3 ), OnStartQueueRetry )
                .ExecuteAsync( async () => await mQueueListProvider.StartQueueStatusRequests());
        }

        private void OnStartQueueRetry( DelegateResult<bool> result, TimeSpan delay, Context context ) {
            mLog.LogMessage( "Restarting queue status requests" );
        }

        private void OnQueueChanged( QueueStatusResponse queueList ) {
            try {
                mQueueList.Edit( list => {
                    list.Clear();

                    if( queueList?.QueueList != null ) {
                        var shortenedList = new List<QueueTrackInfo>( queueList.QueueList );

                        // limit the opening number of played tracks to suite a smaller display
                        if( shortenedList.Count > 4 ) {
                            while( shortenedList.Take( 4 ).All( t => t.HasPlayed )) {
                                shortenedList.RemoveAt( 0 );
                            }
                        }

                        list.AddRange( from q in shortenedList select new UiQueuedTrack( q ));

                        TotalPlayingTime = TimeSpan.FromMilliseconds( queueList.TotalPlayMilliseconds );
                        RemainingPlayTime = TimeSpan.FromMilliseconds( queueList.RemainingPlayMilliseconds );
                    }
                });

                UpdatePlayingTracks();
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( OnQueueChanged ), ex );
            }
        }

        private void UpdatePlayingTracks() {
            mNextPlayingTrack.OnNext( mQueueList.Items.FirstOrDefault( t => !t.HasPlayed && !t.IsPlaying ));

            mPlayingState.OnNext( new PlayingState( mQueueList.Items.FirstOrDefault( t => t.IsPlaying )));
        }

        public void Dispose() {
            mLibraryStatusSubscription?.Dispose();
            mLibraryStatusSubscription = null;

            mClientStatusSubscription?.Dispose();
            mClientStatusSubscription = null;
        }
    }
}
