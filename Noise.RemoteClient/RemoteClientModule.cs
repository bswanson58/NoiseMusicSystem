using Noise.RemoteClient.Interfaces;
using Noise.RemoteClient.Models;
using Noise.RemoteClient.Services;
using Noise.RemoteClient.ViewModels;
using Noise.RemoteClient.Views;
using Prism.Ioc;
using Xamarin.Forms;

namespace Noise.RemoteClient {
    static class RemoteClientModule {
        public static void RegisterServices( IContainerRegistry container ) {
            container.RegisterSingleton<IClientManager, ClientManager>();
            container.RegisterSingleton<IServiceLocator, ServiceLocator>();
            container.RegisterSingleton<IHostInformationProvider, HostInformationProvider>();

            container.Register<IArtistProvider, ArtistProvider>();
            container.Register<IAlbumProvider, AlbumProvider>();
            container.Register<ITrackProvider, TrackProvider>();
        }

        public static void RegisterNavigation( IContainerRegistry container ) {
            container.RegisterForNavigation<NavigationPage>();
            container.RegisterForNavigation<ArtistList, ArtistListViewModel>();
            container.RegisterForNavigation<AlbumList, AlbumListViewModel>();
            container.RegisterForNavigation<MainPage, MainPageViewModel>();
            container.RegisterForNavigation<TrackList, TrackListViewModel>();
        }
    }
}
