using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.EntityFrameworkDatabase.DatabaseUtility;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.EntityFrameworkDatabase.DatabaseManager;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.EntityFrameworkDatabase.Logging;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase {
	public class EntityFrameworkDatabaseModule : IModule {
		private readonly IUnityContainer    mContainer;

		public EntityFrameworkDatabaseModule( IUnityContainer container ) {
			mContainer = container;
		}

		public void Initialize() {
			mContainer.RegisterType<IDatabaseManager, EntityFrameworkDatabaseManager>(  new HierarchicalLifetimeManager());
			mContainer.RegisterType<IContextProvider, ContextProvider>( new HierarchicalLifetimeManager());
#if DEBUG
			mContainer.RegisterType<IDatabaseInitializeStrategy, DebugDatabaseInitialize>();
#else
			mContainer.RegisterType<IDatabaseInitializeStrategy, ProductionDatabaseInitialize>();
#endif

			mContainer.RegisterType<IDbBaseProvider, DbBaseProvider>();
			mContainer.RegisterType<IArtistProvider, ArtistProvider>();
			mContainer.RegisterType<IAlbumProvider, AlbumProvider>();
			mContainer.RegisterType<ITrackProvider, TrackProvider>();
			mContainer.RegisterType<IInternetStreamProvider, InternetStreamProvider>();
			mContainer.RegisterType<IArtworkProvider, ArtworkProvider>();
			mContainer.RegisterType<IGenreProvider, GenreProvider>();
			mContainer.RegisterType<ILyricProvider, LyricProvider>();
			mContainer.RegisterType<IPlayHistoryProvider, PlayHistoryProvider>();
			mContainer.RegisterType<IPlayListProvider, PlayListProvider>();
			mContainer.RegisterType<IRootFolderProvider, RootFolderProvider>();
			mContainer.RegisterType<ISidecarProvider, SidecarProvider>();
			mContainer.RegisterType<IStorageFileProvider, StorageFileProvider>();
			mContainer.RegisterType<IStorageFolderProvider, StorageFolderProvider>();
			mContainer.RegisterType<ITagProvider, TagProvider>();
			mContainer.RegisterType<ITagAssociationProvider, TagAssociationProvider>();
			mContainer.RegisterType<ITextInfoProvider, TextInfoProvider>();
			mContainer.RegisterType<ITimestampProvider, TimestampProvider>();
			mContainer.RegisterType<IDatabaseInfo, DbVersionProvider>();

			mContainer.RegisterType<IDatabaseUtility, SqlDatabaseManager>();

			mContainer.RegisterType<ILogDatabase, LogDatabase>( new HierarchicalLifetimeManager());
		}
	}
}
