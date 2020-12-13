using System;
using System.Collections;
using System.Collections.ObjectModel;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;
using Prism.Mvvm;

namespace Noise.RemoteClient.ViewModels {
    class QueueViewModel : BindableBase, IDisposable {
        private readonly IQueueListProvider mQueueListProvider;
        private IDisposable                 mLibraryStatusSubscription;
        private IDisposable                 mQueueSubscription;

        public  ObservableCollection<UiQueuedTrack> QueueList { get; }

        public QueueViewModel( IQueueListProvider queueListProvider, IHostInformationProvider hostInformationProvider ) {
            mQueueListProvider = queueListProvider;

            QueueList = new ObservableCollection<UiQueuedTrack>();
            Xamarin.Forms.BindingBase.EnableCollectionSynchronization( QueueList, null, ObservableCollectionCallback);

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

        public void Dispose() {
            mLibraryStatusSubscription?.Dispose();
            mLibraryStatusSubscription = null;

            mQueueSubscription?.Dispose();
            mQueueSubscription = null;
        }
    }
}
