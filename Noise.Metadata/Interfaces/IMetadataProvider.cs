using Noise.Infrastructure.Interfaces;
using Raven.Client;

namespace Noise.Metadata.Interfaces {
	public interface IArtistMetadataProvider : IMetadataProvider {
		void		UpdateArtist( string artistName );
	}

	public interface IMetadataProvider {
		string		ProviderKey { get; }

		void		Initialize( IDocumentStore documentStore, ILicenseManager licenseManager );
		void		Shutdown();
	}
}
