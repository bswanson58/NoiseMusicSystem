using System.ServiceModel;
using System.ServiceModel.Web;
using Noise.Infrastructure.RemoteDto;

namespace Noise.Infrastructure.RemoteHost {
	[ServiceContract]
	public interface INoiseRemoteData {
		[OperationContract]
		[WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "artists")]
		ArtistListResult GetArtistList();

		[OperationContract]
		[WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "artist?artist={artistId}")]
		ArtistInfoResult GetArtistInfo( long artistId );

		[OperationContract]
		[WebGet( ResponseFormat = WebMessageFormat.Json, UriTemplate = "albums?artist={artistId}" )]
		AlbumListResult GetAlbumList( long artistId );

		[OperationContract]
		[WebGet( ResponseFormat = WebMessageFormat.Json, UriTemplate = "album?album={albumId}" )]
		AlbumInfoResult GetAlbumInfo( long albumId );

		[OperationContract]
		[WebGet( ResponseFormat = WebMessageFormat.Json, UriTemplate = "tracks?album={albumId}" )]
		TrackListResult GetTrackList( long albumId );

		[OperationContract]
		[WebGet( ResponseFormat = WebMessageFormat.Json, UriTemplate = "favorites" )]
		FavoriteListResult GetFavoriteList();

		[OperationContract]
		[WebGet( ResponseFormat = WebMessageFormat.Json, UriTemplate = "artistTracks?artist={artistId}" )]
		ArtistTracksResult GetArtistTrackList( long artistId );
	}
}
