using Noise.RemoteClient.Interfaces;

namespace Noise.RemoteClient.Services {
    class ArtistProvider : IArtistProvider {
        private readonly IServiceLocator    mServiceLocator;

        public ArtistProvider( IServiceLocator serviceLocator ) {
            mServiceLocator = serviceLocator;
        }
    }
}
