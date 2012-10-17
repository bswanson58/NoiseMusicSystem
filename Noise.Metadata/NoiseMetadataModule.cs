using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.ArtistMetadata;
using Noise.Metadata.Interfaces;

namespace Noise.Metadata {
	public class NoiseMetadataModule : IModule {
		private readonly IUnityContainer    mContainer;

		public NoiseMetadataModule( IUnityContainer container ) {
			mContainer = container;
		}

		public void Initialize() {
			mContainer.RegisterType<IRequireConstruction, MetadataManager>( "MetadataManager", new HierarchicalLifetimeManager());
			mContainer.RegisterType<IArtistMetadataManager, ArtistMetadataManager>();
		}
	}
}
