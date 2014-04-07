using System.Runtime.Serialization;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoPlayHistory {
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
		[DataMember]
		public long			PlayedOnTicks { get; set; }

		public RoPlayHistory() {
			Artist = string.Empty;
			Album = string.Empty;
			Track = string.Empty;
		}

		public RoPlayHistory( DbArtist artist, DbAlbum album, DbTrack track, long playedOn ) {
			ArtistId = artist.DbId;
			Artist = artist.Name;
			AlbumId = album.DbId;
			Album = album.Name;
			TrackId = track.DbId;
			Track = track.Name;
			PlayedOnTicks = playedOn;
		}
	}
}
