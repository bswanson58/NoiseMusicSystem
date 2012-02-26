using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.EntityFrameworkDatabase.DatabaseManager;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase {
	public class EntityFrameworkDatabaseModule : IModule {
		private readonly IUnityContainer    mContainer;

		public EntityFrameworkDatabaseModule( IUnityContainer container ) {
			mContainer = container;
		}

		public void Initialize() {
			var config = NoiseSystemConfiguration.Current.RetrieveConfiguration<DatabaseConfiguration>( DatabaseConfiguration.SectionName );
			mContainer.RegisterInstance( config );

			mContainer.RegisterType<IDatabaseManager, EntityFrameworkDatabaseManager>();
			mContainer.RegisterType<IDatabaseInitializeStrategy, DebugDatabaseInitialize>();

//			mContainer.RegisterType<IDbBaseProvider, DbBaseProvider>();
//			mContainer.RegisterType<IArtistProvider, ArtistProvider>();
//			mContainer.RegisterType<IAlbumProvider, AlbumProvider>();
			mContainer.RegisterType<ITrackProvider, TrackProvider>();
			mContainer.RegisterType<IInternetStreamProvider, InternetStreamProvider>();
//			mContainer.RegisterType<IArtworkProvider, ArtworkProvider>();
			mContainer.RegisterType<IDiscographyProvider, DiscographyProvider>();
//			mContainer.RegisterType<IExpiringContentProvider, ExpiringContentProvider>();
			mContainer.RegisterType<IGenreProvider, GenreProvider>();
			mContainer.RegisterType<ILyricProvider, LyricProvider>();
			mContainer.RegisterType<IPlayHistoryProvider, PlayHistoryProvider>();
//			mContainer.RegisterType<IPlayListProvider, PlayListProvider>();
//			mContainer.RegisterType<IRootFolderProvider, RootFolderProvider>();
//			mContainer.RegisterType<IStorageFileProvider, StorageFileProvider>();
//			mContainer.RegisterType<IStorageFolderProvider, StorageFolderProvider>();
//			mContainer.RegisterType<ITagProvider, TagProvider>();
//			mContainer.RegisterType<ITagAssociationProvider, TagAssociationProvider>();
//			mContainer.RegisterType<ITextInfoProvider, TextInfoProvider>();
//			mContainer.RegisterType<ITimestampProvider, TimestampProvider>();
			mContainer.RegisterType<IAssociatedItemListProvider, AssociatedItemListProvider>();
		}
	}
}
