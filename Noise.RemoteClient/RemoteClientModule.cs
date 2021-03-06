﻿using Noise.RemoteClient.Dialogs;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteClient.Models;
using Noise.RemoteClient.Platform;
using Noise.RemoteClient.Services;
using Noise.RemoteClient.Support;
using Noise.RemoteClient.Views;
using Prism.Ioc;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials.Implementation;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace Noise.RemoteClient {
    static class RemoteClientModule {
        public static void RegisterServices( IContainerRegistry container ) {
            container.RegisterSingleton<IClientEnvironment, ClientEnvironment>();
            container.RegisterSingleton<IPreferences, PreferencesImplementation>();

            container.RegisterInstance( PopupNavigation.Instance );

            container.RegisterSingleton<IClientManager, ClientManager>();
            container.RegisterSingleton<IClientState, ClientState>();
            container.RegisterSingleton<IPlatformLog, SeriLogAdapter>();
            container.RegisterSingleton<IQueuedItemNotifier, QueuedItemNotifier>();
            container.RegisterSingleton<IQueueListener, QueueListener>();
            container.RegisterSingleton<IServiceLocator, ServiceLocator>();
            container.RegisterSingleton<IHostInformationProvider, HostInformationProvider>();
            container.RegisterSingleton<IQueueListProvider, QueueListProvider>();
            container.RegisterSingleton<IQueuePlayProvider, QueuePlayProvider>();

            container.Register<IArtistProvider, ArtistProvider>();
            container.Register<IAlbumProvider, AlbumProvider>();
            container.Register<ICssStyleProvider, CssStyleProvider>();
            container.Register<IPrefixedNameHandler, PrefixedNameHandler>();
            container.Register<ISearchProvider, SearchProvider>();
            container.Register<ITagInformationProvider, TagInformationProvider>();
            container.Register<ITrackProvider, TrackProvider>();
            container.Register<ITransportProvider, TransportProvider>();
        }

        public static void RegisterNavigation( IContainerRegistry container ) {
            Routing.RegisterRoute( RouteNames.ArtistList, typeof( ArtistList ));
            Routing.RegisterRoute( RouteNames.AlbumList, typeof( AlbumList ));
            Routing.RegisterRoute( RouteNames.TrackList, typeof( TrackList ));
            Routing.RegisterRoute( RouteNames.LogFileDisplay, typeof( LogDisplay ));

            // do not register fly out content...

            container.RegisterDialog<EditAlbumRatingsView, EditAlbumRatingsViewModel>();
            container.RegisterDialog<EditTrackRatingsView, EditTrackRatingsViewModel>();
            container.RegisterDialog<EditTrackTagsView, EditTrackTagsViewModel>();
        }
    }
}
