using System;
using System.Linq;
using DynamicData;
using DynamicData.Binding;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;
using Prism.Commands;
using Prism.Mvvm;
using Xamarin.Forms;

namespace Noise.RemoteClient.ViewModels {
    class QueueViewModel : BindableBase, IDisposable {
        private readonly IQueueListProvider         mQueueListProvider;
        private readonly ITransportProvider         mTransportProvider;
        private readonly IClientState               mClientState;
        private readonly SourceList<UiQueuedTrack>  mQueueList;
        private IDisposable                         mLibraryStatusSubscription;
        private IDisposable                         mQueueSubscription;
        private IDisposable                         mListSubscription;

        public  ObservableCollectionExtended<UiQueuedTrack> QueueList { get; }

        public  TimeSpan                            TotalTime { get; private set; }
        public  TimeSpan                            RemainingTime { get; private set; }

        public  DelegateCommand<UiQueuedTrack>      Suggestions { get; }

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

        public QueueViewModel( IQueueListProvider queueListProvider, ITransportProvider transportProvider, 
                               IHostInformationProvider hostInformationProvider, IClientState clientState ) {
            mQueueListProvider = queueListProvider;
            mTransportProvider = transportProvider;
            mClientState = clientState;

            QueueList = new ObservableCollectionExtended<UiQueuedTrack>();
            mQueueList = new SourceList<UiQueuedTrack>();
            mListSubscription = mQueueList.Connect().Bind( QueueList ).Subscribe();

            Suggestions = new DelegateCommand<UiQueuedTrack>( OnSuggestions );

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

            mLibraryStatusSubscription = hostInformationProvider.LibraryStatus.Subscribe( OnHostStatus );
        }

        private void OnHostStatus( LibraryStatus status ) {
            if( status?.LibraryOpen == true ) {
                mQueueSubscription = mQueueListProvider.QueueListStatus.Subscribe( OnQueueChanged );
                mQueueListProvider.StartQueueStatusRequests();
            }
            else {
                mQueueListProvider.StopQueueStatusRequests();

                mQueueSubscription?.Dispose();
                mQueueSubscription = null;

                QueueList.Clear();
            }
        }

        private void OnQueueChanged( QueueStatusResponse queueList ) {
            mQueueList.Clear();

            if( queueList?.QueueList != null ) {
                mQueueList.AddRange( from q in queueList.QueueList select new UiQueuedTrack( q ));

                TotalTime = TimeSpan.FromMilliseconds( queueList.TotalPlayMilliseconds );
                RemainingTime = TimeSpan.FromMilliseconds( queueList.RemainingPlayMilliseconds );

                RaisePropertyChanged( nameof( TotalTime ));
                RaisePropertyChanged( nameof( RemainingTime ));
            }
        }

        private async void OnSuggestions( UiQueuedTrack forTrack ) {
            mClientState.SetSuggestionState( forTrack );

            // route to the shell content page, do push it on the navigation stack.
            await Shell.Current.GoToAsync( "///suggestions" );
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
            mLibraryStatusSubscription?.Dispose();
            mLibraryStatusSubscription = null;

            mQueueSubscription?.Dispose();
            mQueueSubscription = null;

            mListSubscription?.Dispose();
            mListSubscription = null;
        }
    }
}
