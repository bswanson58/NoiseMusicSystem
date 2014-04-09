using System.Runtime.Serialization;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoPlayHistory : RoTrack {
		[DataMember]
		public long			PlayedOnTicks { get; set; }

		public RoPlayHistory( DbArtist artist, DbAlbum album, DbTrack track, long playedOn ) :
			base( artist, album, track ) {
			PlayedOnTicks = playedOn;
		}
	}
}
