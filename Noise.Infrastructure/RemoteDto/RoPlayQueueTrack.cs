using System;
using System.Runtime.Serialization;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoPlayQueueTrack {
		[DataMember]
		public long		Id {  get; set; }
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

		public RoPlayQueueTrack( PlayQueueTrack track ) {
			Id = track.Uid;
			TrackId = track.Track.DbId;
			TrackName = track.Track.Name;
			AlbumId = track.Album.DbId;
			AlbumName = track.Album.Name;
			ArtistId = track.Artist.DbId;
			ArtistName = track.Artist.Name;
			DurationMilliseconds = track.Track.DurationMilliseconds;
			IsPlaying = track.IsPlaying;
			HasPlayed = track.HasPlayed;
			IsFaulted = track.IsFaulted;
			IsStrategySourced = track.StrategySource != eStrategySource.User;
        }
	}
}
