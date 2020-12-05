using Noise.Infrastructure.RemoteHost;
using Noise.RemoteServer.Interfaces;

namespace Noise.RemoteServer.Server {
    class RemoteServer : IRemoteServer {
        private readonly IDiscoveryService  mDiscoveryService;

        public RemoteServer( IDiscoveryService discoveryService ) {
            mDiscoveryService = discoveryService;
        }

        public void OpenRemoteServer() {
            mDiscoveryService.StartDiscoveryService();
        }

        public void CloseRemoteServer() {
            mDiscoveryService.StopDiscoveryService();
        }
    }
}
