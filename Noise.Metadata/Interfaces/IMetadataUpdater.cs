using Raven.Client;

namespace Noise.Metadata.Interfaces {
	public interface IMetadataUpdater {
		void	Initialize( IDocumentStore documentStore );
		void	Shutdown();
	}
}
