using System;
using System.Collections;
using System.Collections.ObjectModel;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;
using Prism.Commands;
using Prism.Mvvm;

namespace Noise.RemoteClient.ViewModels {
    class QueueViewModel : BindableBase, IDisposable {
        private readonly IQueueListProvider mQueueListProvider;
        private readonly ITransportProvider mTransportProvider;
        private IDisposable                 mLibraryStatusSubscription;
        private IDisposable                 mQueueSubscription;

        public  ObservableCollection<UiQueuedTrack> QueueList { get; }

        public  DelegateCommand                     Play { get; }
        public  DelegateCommand                     Pause { get; }
        public  DelegateCommand                     Stop { get; }
        public  DelegateCommand                     PlayNext { get; }
        public  DelegateCommand                     PlayPrevious { get; }
        public  DelegateCommand                     ReplayTrack { get; }

        public QueueViewModel( IQueueListProvider queueListProvider, ITransportProvider transportProvider, IHostInformationProvider hostInformationProvider ) {
            mQueueListProvider = queueListProvider;
            mTransportProvider = transportProvider;

            QueueList = new ObservableCollection<UiQueuedTrack>();
            Xamarin.Forms.BindingBase.EnableCollectionSynchronization( QueueList, null, ObservableCollectionCallback);

            Play = new DelegateCommand( OnPlay );
            Pause = new DelegateCommand( OnPause );
            Stop = new DelegateCommand( OnStop );
            PlayPrevious = new DelegateCommand( OnPlayPrevious );
            PlayNext = new DelegateCommand( OnPlayNext );
            ReplayTrack = new DelegateCommand( OnReplayTrack );

            mLibraryStatusSubscription = hostInformationProvider.LibraryStatus.Subscribe( OnHostStatus );
        }

        void ObservableCollectionCallback( IEnumerable collection, object context, Action accessMethod, bool writeAccess ) {
            lock( collection ) {
                accessMethod?.Invoke();
            }
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
            QueueList.Clear();

            if( queueList?.QueueList != null ) {
                foreach( var track in queueList.QueueList ) {
                    QueueList.Add( new UiQueuedTrack( track ));
                }
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

        private void OnReplayTrack() {
            mTransportProvider.ReplayTrack();
        }

        public void Dispose() {
            mLibraryStatusSubscription?.Dispose();
            mLibraryStatusSubscription = null;

            mQueueSubscription?.Dispose();
            mQueueSubscription = null;
        }
    }
}
