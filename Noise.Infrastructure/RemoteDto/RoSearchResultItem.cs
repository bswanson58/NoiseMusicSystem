using System;
using System.Runtime.Serialization;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoSearchResultItem {
		[DataMember]
		public long		TrackId { get; set; }
		[DataMember]
		public string	TrackName { get; set; }
		[DataMember]
		public long		AlbumId { get; set; }
		[DataMember]
		public string	AlbumName { get; set; }
		[DataMember]
		public long		ArtistId { get; set; }
		[DataMember]
		public string	ArtistName { get; set; }
		[DataMember]
		public bool		CanPlay { get; set; }

		public RoSearchResultItem( SearchResultItem src ) {
            ArtistId = src.Artist?.DbId ?? Constants.cDatabaseNullOid;
            ArtistName = src.Artist != null ? src.Artist.Name : String.Empty;
            AlbumId = src.Album?.DbId ?? Constants.cDatabaseNullOid;
            AlbumName = src.Album != null ? src.Album.Name : String.Empty;
            TrackId = src.Track?.DbId ?? Constants.cDatabaseNullOid;
            TrackName = src.Track != null ? src.Track.Name : String.Empty;

            CanPlay = false;
        }
	}
}
