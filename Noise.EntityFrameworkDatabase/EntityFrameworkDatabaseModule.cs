using Noise.EntityFrameworkDatabase.DatabaseUtility;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.EntityFrameworkDatabase.DatabaseManager;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.EntityFrameworkDatabase.Logging;
using Noise.Infrastructure.Interfaces;
using Prism.Ioc;
using Prism.Modularity;

namespace Noise.EntityFrameworkDatabase {
	public class EntityFrameworkDatabaseModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
			containerRegistry.RegisterSingleton<IDatabaseManager, EntityFrameworkDatabaseManager>();
			containerRegistry.RegisterSingleton<IContextProvider, ContextProvider>();
            containerRegistry.RegisterSingleton<IDatabaseInfo, DbVersionProvider>();

#if DEBUG
			containerRegistry.Register<IDatabaseInitializeStrategy, DebugDatabaseInitialize>();
#else
			containerRegistry.Register<IDatabaseInitializeStrategy, ProductionDatabaseInitialize>();
#endif

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
			containerRegistry.Register<ISidecarProvider, SidecarProvider>();
			containerRegistry.Register<IStorageFileProvider, StorageFileProvider>();
			containerRegistry.Register<IStorageFolderProvider, StorageFolderProvider>();
			containerRegistry.Register<ITagProvider, TagProvider>();
			containerRegistry.Register<ITagAssociationProvider, TagAssociationProvider>();
			containerRegistry.Register<ITextInfoProvider, TextInfoProvider>();
			containerRegistry.Register<ITimestampProvider, TimestampProvider>();

			containerRegistry.Register<IDatabaseUtility, SqlDatabaseManager>();
			
            containerRegistry.Register<ILogDatabase, LogDatabase>();
		}

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
