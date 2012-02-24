using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.EloqueraDatabase.BlobStore;
using Noise.EloqueraDatabase.DataProviders;
using Noise.EloqueraDatabase.Database;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase {
	public class EloqueraDatabaseModule : IModule {
		private readonly IUnityContainer    mContainer;

		public EloqueraDatabaseModule( IUnityContainer container ) {
			mContainer = container;
		}

		public void Initialize() {
			var config = NoiseSystemConfiguration.Current.RetrieveConfiguration<DatabaseConfiguration>( DatabaseConfiguration.SectionName );
			mContainer.RegisterInstance( config );

			mContainer.RegisterType<IDatabaseFactory, EloqueraDatabaseFactory>();
			mContainer.RegisterType<IBlobStorageResolver, BlobStorageResolver>();
			mContainer.RegisterType<IDatabaseManager, DatabaseManager>( new HierarchicalLifetimeManager());

			mContainer.RegisterType<IDatabaseInfo, DatabaseInfoProvider>();
			mContainer.RegisterType<IDbBaseProvider, DbBaseProvider>();
			mContainer.RegisterType<IArtistProvider, ArtistProvider>();
			mContainer.RegisterType<IAlbumProvider, AlbumProvider>();
			mContainer.RegisterType<ITrackProvider, TrackProvider>();
			mContainer.RegisterType<IInternetStreamProvider, InternetStreamProvider>();
			mContainer.RegisterType<IArtworkProvider, ArtworkProvider>();
			mContainer.RegisterType<IDiscographyProvider, DbDiscographyProvider>();
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
			mContainer.RegisterType<IAssociatedItemListProvider, AssociatedItemListProvider>();

		}
	}
}
