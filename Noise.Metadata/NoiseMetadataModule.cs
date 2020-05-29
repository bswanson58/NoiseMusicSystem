using System.Collections.Generic;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.ArtistMetadata;
using Noise.Metadata.Interfaces;
using Noise.Metadata.Logging;
using Noise.Metadata.MetadataProviders;
using Noise.Metadata.MetadataProviders.Discogs;
using Noise.Metadata.MetadataProviders.LastFm;
using Prism.Ioc;
using Prism.Modularity;

namespace Noise.Metadata {
	public class NoiseMetadataModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
			containerRegistry.RegisterSingleton<IMetadataManager, MetadataManager>();
			containerRegistry.Register<IArtistMetadataManager, ArtistMetadataManager>();

            containerRegistry.Register<IArtistArtworkSelector, ArtistArtworkSelector>();

			containerRegistry.RegisterSingleton<IMetadataUpdater, ArtistMetadataUpdater>( "ArtistMetadataUpdater" );
			containerRegistry.Register<IList<IMetadataUpdater>, IMetadataUpdater[]>();

			containerRegistry.RegisterSingleton<IArtistMetadataProvider, LastFmProvider>( "LastFmProvider" );
			containerRegistry.RegisterSingleton<IArtistMetadataProvider, DiscogsProvider>( "DiscogsProvider" );
			containerRegistry.Register<IList<IArtistMetadataProvider>, IArtistMetadataProvider[]>();

			containerRegistry.Register<IDiscogsClient, DiscogsClient>();
			containerRegistry.Register<ILastFmClient, LastFmClient>();

			containerRegistry.Register<ILogMetadata, MetadataLogging>();
		}

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
