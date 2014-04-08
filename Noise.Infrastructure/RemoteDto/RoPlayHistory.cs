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
		public string		ArtistName { get; set; }
		[DataMember]
		public string		AlbumName { get; set; }
		[DataMember]
		public string		TrackName { get; set; }
		[DataMember]
		public long			PlayedOnTicks { get; set; }

		public RoPlayHistory() {
			ArtistName = string.Empty;
			AlbumName = string.Empty;
			TrackName = string.Empty;
		}

		public RoPlayHistory( DbArtist artist, DbAlbum album, DbTrack track, long playedOn ) {
			ArtistId = artist.DbId;
			ArtistName = artist.Name;
			AlbumId = album.DbId;
			AlbumName = album.Name;
			TrackId = track.DbId;
			TrackName = track.Name;
			PlayedOnTicks = playedOn;
		}
	}
}
