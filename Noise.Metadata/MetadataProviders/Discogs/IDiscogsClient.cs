using System.Threading.Tasks;
using Noise.Metadata.MetadataProviders.Discogs.Rto;

namespace Noise.Metadata.MetadataProviders.Discogs {
	public interface IDiscogsClient {
		Task<DiscogsSearchResult>		ArtistSearch( string artistName );
		Task<DiscogsArtist>				GetArtist( string artistId );
		Task<DiscogsArtistReleaseList>	GetArtistReleases( string artistId );
	}
}
