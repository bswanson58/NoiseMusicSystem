using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.DataProviders;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Logging;
using Noise.RavenDatabase.Support;
using Prism.Ioc;
using Prism.Modularity;

namespace Noise.RavenDatabase {
	public class RavenDatabaseModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
			containerRegistry.RegisterSingleton<RavenDatabaseManager>();
			containerRegistry.Register<IDbFactory, RavenDatabaseManager>();
			containerRegistry.Register<IDatabaseManager, RavenDatabaseManager>();

			containerRegistry.Register<IDbBaseProvider, DbBaseProvider>();
			containerRegistry.Register<IArtistProvider, ArtistProvider>();
			containerRegistry.Register<IAlbumProvider, AlbumProvider>();
			containerRegistry.Register<ITrackProvider, TrackProvider>();
			containerRegistry.Register<IInternetStreamProvider, InternetStreamProvider>();
			containerRegistry.Register<IArtworkProvider, ArtworkProvider>();
			containerRegistry.Register<IGenreProvider, GenreProvider>();
			containerRegistry.Register<ILyricProvider, LyricProvider>();
			containerRegistry.Register<IPlayHistoryProvider, PlayHistoryProvider>();
			containerRegistry.Register<IPlayListProvider, PlayListProvider>();
			containerRegistry.Register<IRootFolderProvider, RootFolderProvider>();
			containerRegistry.Register<IStorageFileProvider, StorageFileProvider>();
			containerRegistry.Register<IStorageFolderProvider, StorageFolderProvider>();
			containerRegistry.Register<ITagProvider, TagProvider>();
			containerRegistry.Register<ITagAssociationProvider, TagAssociationProvider>();
			containerRegistry.Register<ITextInfoProvider, TextInfoProvider>();
			containerRegistry.Register<ITimestampProvider, TimestampProvider>();
			containerRegistry.Register<IDatabaseInfo, DatabaseInfoProvider>();

			containerRegistry.Register<ILogRaven, RavenLogger>();
		}

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
