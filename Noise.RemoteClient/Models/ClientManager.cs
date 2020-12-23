using System;
using System.Reactive.Subjects;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;

namespace Noise.RemoteClient.Models {
    class ClientManager : IClientManager {
        private readonly IServiceLocator                mServiceLocator;
        private readonly IHostInformationProvider       mHostInformationProvider;
        private readonly IQueuedItemNotifier            mPlayNotifier;
        private readonly IPlatformLog                   mLog;
        private readonly BehaviorSubject<ClientStatus>  mClientStatus;

        public  IObservable<ClientStatus>               ClientStatus => mClientStatus;

        public ClientManager( IServiceLocator serviceLocator, IHostInformationProvider hostInformationProvider, IQueuedItemNotifier playNotifier,
                              IPlatformLog log ) {
            mServiceLocator = serviceLocator;
            mPlayNotifier = playNotifier;
            mHostInformationProvider = hostInformationProvider;
            mLog = log;

            mClientStatus = new BehaviorSubject<ClientStatus>( new ClientStatus( eClientState.Unknown ));
        }

        public void StartClientManager() {
            try {
                mServiceLocator.StartServiceLocator();
                mPlayNotifier.StartNotifications();
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( StartClientManager ), ex );
            }
        }

        public void StopClientManager() {
            try {
                mServiceLocator.StopServiceLocator();
                mHostInformationProvider.StopHostStatusRequests();
                mPlayNotifier.StopNotifications();
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( StopClientManager ), ex );
            }
        }

        public void OnApplicationStarting() {
            StartClientManager();

            try {
                mClientStatus.OnNext( new ClientStatus( eClientState.Starting ));
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( OnApplicationStarting ), ex );
            }
        }

        public void OnApplicationStopping() {
            try {
                mClientStatus.OnNext( new ClientStatus( eClientState.Sleeping ));
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( OnApplicationStopping ), ex );
            }

            StopClientManager();
        }
    }
}
