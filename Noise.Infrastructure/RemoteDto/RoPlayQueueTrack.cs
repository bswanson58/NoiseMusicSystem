using System;
using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoPlayQueueTrack {
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
		public Int32	DurationMilliseconds { get; set; }
		[DataMember]
		public bool		IsPlaying { get; set; }
		[DataMember]
		public bool		HasPlayed { get; set; }
		[DataMember]
		public bool		IsFaulted { get; set; }
		[DataMember]
		public bool		IsStrategySourced { get; set; }
	}
}
