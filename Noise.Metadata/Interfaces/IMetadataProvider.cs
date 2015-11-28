using System.Threading.Tasks;
using Raven.Client;

namespace Noise.Metadata.Interfaces {
	public interface IArtistMetadataProvider : IMetadataProvider {
		Task<bool>	UpdateArtist( string artistName );
	}

	public interface IMetadataProvider {
		string		ProviderKey { get; }

		void		Initialize( IDocumentStore documentStore );
		void		Shutdown();
	}
}
