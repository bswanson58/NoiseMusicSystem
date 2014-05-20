using System.Threading.Tasks;
using Noise.Metadata.MetadataProviders.Discogs.Rto;
using Refit;

namespace Noise.Metadata.MetadataProviders.Discogs {
	[Headers("User-Agent: Noise Music System")]
	public interface IDiscogsApi {
		[Get( "/database/search" )]
		Task<DiscogsSearchResult> Search([AliasAs( "q" )] string query, [AliasAs( "type" )] string searchType );

		[Get( "/artists/{id}" )]
		Task<DiscogsArtist>	GetArtist([AliasAs( "id" )] string artistId );

		[Get( "/artists/{id}/releases" )]
		Task<DiscogsArtistReleases> GetArtistReleases([AliasAs( "id" )] string artistId );
	}
}
