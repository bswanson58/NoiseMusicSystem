using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoTransportState : BaseResult {
		[DataMember]
		public long		ServerTime { get; set; }
		[DataMember]
		public int		PlayState { get; set; }
		[DataMember]
		public long		CurrentTrack { get; set; }
		[DataMember]
		public long		CurrentTrackPosition { get; set; }
		[DataMember]
		public long		CurrentTrackLength { get; set; }
		[DataMember]
		public bool		CanPlay { get; set; }
		[DataMember]
		public bool		CanPause { get; set; }
		[DataMember]
		public bool		CanStop { get; set; }
		[DataMember]
		public bool		CanPlayNextTrack { get; set; }
		[DataMember]
		public bool		CanPlayPreviousTrack { get; set; }
	}
}
