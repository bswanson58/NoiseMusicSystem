using System.Collections.Generic;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.ArtistMetadata;
using Noise.Metadata.Interfaces;
using Noise.Metadata.Logging;
using Noise.Metadata.MetadataProviders;
using Noise.Metadata.MetadataProviders.Discogs;
using Noise.Metadata.MetadataProviders.LastFm;

namespace Noise.Metadata {
	public class NoiseMetadataModule : IModule {
		private readonly IUnityContainer    mContainer;

		public NoiseMetadataModule( IUnityContainer container ) {
			mContainer = container;
		}

		public void Initialize() {
			mContainer.RegisterType<IMetadataManager, MetadataManager>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IArtistMetadataManager, ArtistMetadataManager>();

            mContainer.RegisterType<IArtistArtworkSelector, ArtistArtworkSelector>();

			mContainer.RegisterType<IMetadataUpdater, ArtistMetadataUpdater>( "ArtistMetadataUpdater" );
			mContainer.RegisterType<IEnumerable<IMetadataUpdater>, IMetadataUpdater[]>();

			mContainer.RegisterType<IArtistMetadataProvider, LastFmProvider>( "LastFmProvider" );
			mContainer.RegisterType<IArtistMetadataProvider, DiscogsProvider>( "DiscogsProvider" );
			mContainer.RegisterType<IEnumerable<IArtistMetadataProvider>, IArtistMetadataProvider[]>();

			mContainer.RegisterType<IDiscogsClient, DiscogsClient>();
			mContainer.RegisterType<ILastFmClient, LastFmClient>();

			mContainer.RegisterType<ILogMetadata, MetadataLogging>( new HierarchicalLifetimeManager());
		}
	}
}
