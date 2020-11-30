using System.Threading.Tasks;

namespace Noise.Metadata.Interfaces {
	public interface IArtistMetadataProvider : IMetadataProvider {
		Task<bool>	UpdateArtist( string artistName );
	}

	public interface IMetadataProvider {
		string		ProviderKey { get; }
	}
}
