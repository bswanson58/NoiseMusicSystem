using System.Threading.Tasks;
using Noise.Metadata.MetadataProviders.Discogs.Rto;
using Refit;

namespace Noise.Metadata.MetadataProviders.Discogs {
	[Headers("User-Agent: Noise Music System")]
	public interface IDiscogsApi {
		[Get( "/database/search" )]
		Task<DiscogsSearchResult> Search([AliasAs( "q" )] string query, [AliasAs( "type" )] string searchType,[AliasAs( "token" )] string token );

		[Get( "/artists/{id}" )]
		Task<DiscogsArtist>	GetArtist([AliasAs( "id" )] string artistId,[AliasAs( "token" )] string token );

		[Get( "/artists/{id}/releases" )]
		Task<DiscogsArtistReleaseList> GetArtistReleases([AliasAs( "id" )] string artistId,[AliasAs( "token" )] string token );
	}
}
