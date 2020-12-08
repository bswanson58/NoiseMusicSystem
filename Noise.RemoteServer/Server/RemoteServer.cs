using System.Net;
using Grpc.Core;
using Noise.Infrastructure.RemoteHost;
using Noise.RemoteServer.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteServer.Server {
    class RemoteServer : IRemoteServer {
        private readonly IDiscoveryService          mDiscoveryService;
        private readonly RemoteHostConfiguration    mRemoteHostConfiguration;
        private readonly IRemoteServiceFactory      mServiceFactory;
        private Grpc.Core.Server	                mNoiseServer;

        public RemoteServer( IDiscoveryService discoveryService, IRemoteServiceFactory serviceFactory, RemoteHostConfiguration remoteHostConfiguration ) {
            mDiscoveryService = discoveryService;
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

            mNoiseServer.Services.Add( HostInformation.BindService( mServiceFactory.HostInformationService ));
            mNoiseServer.Ports.Add( new ServerPort( Dns.GetHostName(), mRemoteHostConfiguration.HostPort, ServerCredentials.Insecure ));
            mNoiseServer.Start();
        }

        public void CloseRemoteServer() {
            mDiscoveryService.StopDiscoveryService();

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
