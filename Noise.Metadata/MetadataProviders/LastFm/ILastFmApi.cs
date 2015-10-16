using System.Threading.Tasks;
using Noise.Metadata.MetadataProviders.LastFm.Rto;
using Refit;

namespace Noise.Metadata.MetadataProviders.LastFm {
	internal interface ILastFmApi {
		[Get( "/2.0/?method=artist.search" )]
		Task<LastFmAristSearch>		ArtistSearch([AliasAs( "artist" )] string artistName, [AliasAs( "api_key" )] string apiKey, [AliasAs( "format" )] string format );

		[Get( "/2.0/?method=artist.getinfo" )]
		Task<LastFmArtistInfoResult>	GetArtistInfo([AliasAs( "artist" )] string artistName, [AliasAs( "api_key" )] string apiKey, [AliasAs( "format" )] string format );

		[Get( "/2.0/?method=artist.gettopalbums" )]
		Task<LastFmTopAlbums>			GetTopAlbums([AliasAs( "artist" )] string artistName, [AliasAs( "api_key" )] string apiKey, [AliasAs( "format" )] string format );

		[Get( "/2.0/?method=artist.gettoptracks" )]
		Task<LastFmTopTracks>			GetTopTracks([AliasAs( "artist" )] string artistName, [AliasAs( "api_key" )] string apiKey, [AliasAs( "format" )] string format );
	}
}
