using System.Threading.Tasks;
using Noise.Metadata.MetadataProviders.LastFm.Rto;

namespace Noise.Metadata.MetadataProviders.LastFm {
	internal interface ILastFmClient {
		Task<LastFmArtistList>	ArtistSearch( string artistName );
		Task<LastFmArtistInfo>	GetArtistInfo( string artistName );
		Task<LastFmAlbumList>	GetTopAlbums( string artistName );
		Task<LastFmTrackList>	GetTopTracks( string artistName );
	}
}
