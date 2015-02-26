using System.Threading.Tasks;
using Noise.Metadata.MetadataProviders.LastFm.Rto;

namespace Noise.Metadata.MetadataProviders.LastFm {
	internal interface ILastFmClient {
		Task<LastFmArtistList>	ArtistSearch( string artistName );
		Task<LastFmArtistInfo>	GetArtistInfo( string artistName );
		Task<LastFmTopAlbums>	GetTopAlbums( string artistName );
		Task<LastFmTopTracks>	GetTopTracks( string artistName );
	}
}
