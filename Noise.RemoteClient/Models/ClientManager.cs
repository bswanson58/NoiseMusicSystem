using Noise.RemoteClient.Interfaces;

namespace Noise.RemoteClient.Models {
    class ClientManager : IClientManager {
        private readonly IServiceLocator    mServiceLocator;

        public ClientManager( IServiceLocator serviceLocator ) {
            mServiceLocator = serviceLocator;
        }

        public void StartClientManager() {
            mServiceLocator.StartServiceLocator();
        }

        public void StopClientManager() {
            mServiceLocator.StopServiceLocator();
        }
    }
}
