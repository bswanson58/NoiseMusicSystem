using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.EntityFrameworkDatabase.DatabaseManager;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase {
	public class EntityFrameworkDatabaseModule : IModule {
		private readonly IUnityContainer    mContainer;

		public EntityFrameworkDatabaseModule( IUnityContainer container ) {
			mContainer = container;
		}

		public void Initialize() {
			mContainer.RegisterType<IDatabaseManager, EntityFrameworkDatabaseManager>();
			mContainer.RegisterType<IContextProvider, ContextProvider>();
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
			mContainer.RegisterType<IExpiringContentProvider, ExpiringContentProvider>();
			mContainer.RegisterType<IGenreProvider, GenreProvider>();
			mContainer.RegisterType<ILyricProvider, LyricProvider>();
			mContainer.RegisterType<IPlayHistoryProvider, PlayHistoryProvider>();
			mContainer.RegisterType<IPlayListProvider, PlayListProvider>();
			mContainer.RegisterType<IRootFolderProvider, RootFolderProvider>();
			mContainer.RegisterType<IStorageFileProvider, StorageFileProvider>();
			mContainer.RegisterType<IStorageFolderProvider, StorageFolderProvider>();
			mContainer.RegisterType<ITagProvider, TagProvider>();
			mContainer.RegisterType<ITagAssociationProvider, TagAssociationProvider>();
			mContainer.RegisterType<ITextInfoProvider, TextInfoProvider>();
			mContainer.RegisterType<ITimestampProvider, TimestampProvider>();
			mContainer.RegisterType<IDatabaseInfo, DbVersionProvider>();
		}
	}
}
