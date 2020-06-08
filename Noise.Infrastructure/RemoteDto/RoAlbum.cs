using System;
using System.Runtime.Serialization;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoAlbum : RoBase {
		[DataMember]
		public string			Name { get; set; }
		[DataMember]
		public long				ArtistId { get; set; }
		[DataMember]
		public Int16			TrackCount { get; set; }
		[DataMember]
		public Int16			Rating { get; set; }
		[DataMember]
		public Int32			PublishedYear { get; set; }
		[DataMember]
		public string			Genre { get; set; }
		[DataMember]
		public bool				IsFavorite { get; set; }

		public RoAlbum( DbAlbum album ) {
			DbId = album.DbId;
			Name = album.Name;
			ArtistId = album.Artist;
			TrackCount = album.TrackCount;
			Rating = album.Rating;
			PublishedYear = album.PublishedYear;
			IsFavorite = album.IsFavorite;

			Genre = String.Empty;
        }
	}
}
