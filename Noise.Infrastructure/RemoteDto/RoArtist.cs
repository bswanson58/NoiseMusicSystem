using System;
using System.Runtime.Serialization;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoArtist : RoBase {
		[DataMember]
		public string			Name { get; set; }
		[DataMember]
		public Int16			AlbumCount { get; set; }
		[DataMember]
		public Int16			Rating { get; set; }
		[DataMember]
		public string			Genre { get; set; }
		[DataMember]
		public bool				IsFavorite { get; set; }

		public RoArtist( DbArtist artist ) {
			DbId = artist.DbId;
			Name = artist.Name;
			AlbumCount = artist.AlbumCount;
			Rating = artist.Rating;
			IsFavorite = artist.IsFavorite;

			Genre = String.Empty;
        }
	}
}
