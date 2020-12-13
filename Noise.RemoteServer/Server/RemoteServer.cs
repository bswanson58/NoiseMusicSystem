using System;
using System.Net;
using Grpc.Core;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteHost;
using Noise.RemoteServer.Interfaces;
using Noise.RemoteServer.Protocol;
using Noise.RemoteServer.Services;

namespace Noise.RemoteServer.Server {
    class RemoteServer : IRemoteServer {
        private readonly IDiscoveryService          mDiscoveryService;
        private readonly HostInformationService     mHostInformationService;
        private readonly RemoteHostConfiguration    mRemoteHostConfiguration;
        private readonly IRemoteServiceFactory      mServiceFactory;
        private readonly INoiseLog                  mLog;
        private Grpc.Core.Server	                mNoiseServer;

        public RemoteServer( IDiscoveryService discoveryService, HostInformationService hostInformationService,
                             IRemoteServiceFactory serviceFactory, RemoteHostConfiguration remoteHostConfiguration, INoiseLog log ) {
            mDiscoveryService = discoveryService;
            mHostInformationService = hostInformationService;
            mServiceFactory = serviceFactory;
            mRemoteHostConfiguration = remoteHostConfiguration;
            mLog = log;
        }

        public void OpenRemoteServer() {
            mDiscoveryService.StartDiscoveryService();

            StartRpcServer();
        }

        private void StartRpcServer() {
            StopRpcServer();

            try {
                mNoiseServer = mServiceFactory.HostServer;

                mNoiseServer.Services.Add( HostInformation.BindService( mHostInformationService ));
                mNoiseServer.Services.Add( ArtistInformation.BindService( mServiceFactory.ArtistInformationService ));
                mNoiseServer.Services.Add( AlbumInformation.BindService( mServiceFactory.AlbumInformationService ));
                mNoiseServer.Services.Add( TrackInformation.BindService( mServiceFactory.TrackInformationService ));
                mNoiseServer.Services.Add( QueueControl.BindService( mServiceFactory.QueueControlService ));
                mNoiseServer.Services.Add( TransportControl.BindService( mServiceFactory.TransportControlService ));
                mNoiseServer.Ports.Add( new ServerPort( Dns.GetHostName(), mRemoteHostConfiguration.HostPort, ServerCredentials.Insecure ));
                mNoiseServer.Start();
            }
            catch( Exception ex ) {
                mLog.LogException( "StartRpcServer", ex );
            }
        }

        public void CloseRemoteServer() {
            try {
                mDiscoveryService.StopDiscoveryService();
                mHostInformationService.StopHostStatusResponder();

                StopRpcServer();
            }
            catch( Exception ex ) {
                mLog.LogException( "CloseRemoteServer", ex );
            }
        }

        private async void StopRpcServer() {
            if( mNoiseServer != null ) {
                try {
                    await mNoiseServer.ShutdownAsync();
                }
                catch( Exception ex ) {
                    mLog.LogException( "StopRpcServer", ex );
                }

                mNoiseServer = null;
            }
        }
    }
}
