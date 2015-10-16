using System;
using System.Runtime.Serialization;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoTrack {
		[DataMember]
		public long			TrackId {  get; set; }
		[DataMember]
		public long			ArtistId { get; set; }
		[DataMember]
		public long			AlbumId { get; set; }
		[DataMember]
		public string		ArtistName { get; set; }
		[DataMember]
		public string		AlbumName { get; set; }
		[DataMember]
		public string		TrackName { get; set; }
		[DataMember]
		public string		VolumeName { get; set; }
		[DataMember]
		public long			Duration { get; set; }
		[DataMember]
		public Int16		Rating { get; set; }
		[DataMember]
		public Int16		TrackNumber { get; set; }
		[DataMember]
		public bool			IsFavorite { get; set; }

		public RoTrack() {
			TrackId = Constants.cDatabaseNullOid;
			AlbumId = Constants.cDatabaseNullOid;
			ArtistId = Constants.cDatabaseNullOid;

			ArtistName = string.Empty;
			AlbumName = string.Empty;
			TrackName = string.Empty;
			VolumeName = string.Empty;
		}

		public RoTrack( DbArtist artist, DbAlbum album, DbTrack track ) {
			ArtistId = artist.DbId;
			ArtistName = artist.Name;
			AlbumId = album.DbId;
			AlbumName = album.Name;
			TrackId = track.DbId;
			TrackName = track.Name;
			VolumeName = track.VolumeName;
			Duration = track.DurationMilliseconds;
			Rating = track.Rating;
			TrackNumber = track.TrackNumber;
			IsFavorite = track.IsFavorite;
		}
	}
}
