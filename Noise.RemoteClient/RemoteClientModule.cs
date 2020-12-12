using Noise.RemoteClient.Interfaces;
using Noise.RemoteClient.Models;
using Noise.RemoteClient.Services;
using Noise.RemoteClient.Views;
using Prism.Ioc;
using Xamarin.Forms;

namespace Noise.RemoteClient {
    static class RemoteClientModule {
        public static void RegisterServices( IContainerRegistry container ) {
            container.RegisterSingleton<IClientManager, ClientManager>();
            container.RegisterSingleton<IClientState, ClientState>();
            container.RegisterSingleton<IServiceLocator, ServiceLocator>();
            container.RegisterSingleton<IHostInformationProvider, HostInformationProvider>();
            container.RegisterSingleton<IQueuePlayProvider, QueuePlayProvider>();

            container.Register<IArtistProvider, ArtistProvider>();
            container.Register<IAlbumProvider, AlbumProvider>();
            container.Register<ITrackProvider, TrackProvider>();
        }

        public static void RegisterNavigation( IContainerRegistry container ) {
            Routing.RegisterRoute( "artistList", typeof( ArtistList ));
            Routing.RegisterRoute( "albumList", typeof( AlbumList ));
            Routing.RegisterRoute( "trackList", typeof( TrackList ));
        }
    }
}
