using Noise.RemoteClient.Interfaces;

namespace Noise.RemoteClient.Models {
    class ClientManager : IClientManager {
        private readonly IServiceLocator            mServiceLocator;
        private readonly IHostInformationProvider   mHostInformationProvider;

        public ClientManager( IServiceLocator serviceLocator, IHostInformationProvider hostInformationProvider ) {
            mServiceLocator = serviceLocator;
            mHostInformationProvider = hostInformationProvider;
        }

        public void StartClientManager() {
            mServiceLocator.StartServiceLocator();
        }

        public void StopClientManager() {
            mServiceLocator.StopServiceLocator();
            mHostInformationProvider.StopHostStatusRequests();
        }
    }
}
