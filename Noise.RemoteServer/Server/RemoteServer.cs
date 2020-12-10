using System.Net;
using Grpc.Core;
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
        private Grpc.Core.Server	                mNoiseServer;

        public RemoteServer( IDiscoveryService discoveryService, HostInformationService hostInformationService,
                             IRemoteServiceFactory serviceFactory, RemoteHostConfiguration remoteHostConfiguration ) {
            mDiscoveryService = discoveryService;
            mHostInformationService = hostInformationService;
            mServiceFactory = serviceFactory;
            mRemoteHostConfiguration = remoteHostConfiguration;
        }

        public void OpenRemoteServer() {
            mDiscoveryService.StartDiscoveryService();

            StartRpcServer();
        }

        private void StartRpcServer() {
            StopRpcServer();

            mNoiseServer = mServiceFactory.HostServer;

            mNoiseServer.Services.Add( HostInformation.BindService( mHostInformationService ));
            mNoiseServer.Services.Add( ArtistInformation.BindService( mServiceFactory.ArtistInformationService ));
            mNoiseServer.Services.Add( AlbumInformation.BindService( mServiceFactory.AlbumInformationService ));
            mNoiseServer.Services.Add( TrackInformation.BindService( mServiceFactory.TrackInformationService ));
            mNoiseServer.Ports.Add( new ServerPort( Dns.GetHostName(), mRemoteHostConfiguration.HostPort, ServerCredentials.Insecure ));
            mNoiseServer.Start();
        }

        public void CloseRemoteServer() {
            mDiscoveryService.StopDiscoveryService();
            mHostInformationService.StopHostStatusResponder();

            StopRpcServer();
        }

        private async void StopRpcServer() {
            if( mNoiseServer != null ) {
                await mNoiseServer.ShutdownAsync();

                mNoiseServer = null;
            }
        }
    }
}
