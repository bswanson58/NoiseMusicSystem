using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.DataProviders;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Logging;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase {
	public class RavenDatabaseModule : IModule {
		private readonly IUnityContainer    mContainer;

		public RavenDatabaseModule( IUnityContainer container ) {
			mContainer = container;
		}

		public void Initialize() {
			mContainer.RegisterType<RavenDatabaseManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IDbFactory, RavenDatabaseManager>();
			mContainer.RegisterType<IDatabaseManager, RavenDatabaseManager>();

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
			mContainer.RegisterType<IStorageFileProvider, StorageFileProvider>();
			mContainer.RegisterType<IStorageFolderProvider, StorageFolderProvider>();
			mContainer.RegisterType<ITagProvider, TagProvider>();
			mContainer.RegisterType<ITagAssociationProvider, TagAssociationProvider>();
			mContainer.RegisterType<ITextInfoProvider, TextInfoProvider>();
			mContainer.RegisterType<ITimestampProvider, TimestampProvider>();
			mContainer.RegisterType<IDatabaseInfo, DatabaseInfoProvider>();

			mContainer.RegisterType<ILogRaven, RavenLogger>( new HierarchicalLifetimeManager());
		}
	}
}
