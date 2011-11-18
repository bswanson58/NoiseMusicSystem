using System.Runtime.Serialization;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoFavorite {
		[DataMember]
		public long			ArtistId { get; set; }
		[DataMember]
		public long			AlbumId { get; set; }
		[DataMember]
		public long			TrackId { get; set; }
		[DataMember]
		public string		Artist { get; set; }
		[DataMember]
		public string		Album { get; set; }
		[DataMember]
		public string		Track { get; set; }

		public RoFavorite() {
			Artist = "";
			Album = "";
			Track = "";
		}

		public RoFavorite( DbArtist artist ) :
			this() {
			ArtistId = artist.DbId;
			Artist = artist.Name;
		}

		public RoFavorite( DbArtist artist, DbAlbum album ) :
			this( artist ) {
			AlbumId = album.DbId;
			Album = album.Name;
		}

		public RoFavorite( DbArtist artist, DbAlbum album, DbTrack track ) :
			this( artist, album ) {
			TrackId = track.DbId;
			Track = track.Name;
		}
	}
}
