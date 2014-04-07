using System;
using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class PlayHistoryListResult : BaseResult {
		[DataMember]
		public	RoPlayHistory[]		PlayHistory { get; set; }
	}
}
