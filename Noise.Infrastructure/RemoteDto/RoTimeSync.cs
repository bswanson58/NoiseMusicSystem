using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoTimeSync : BaseResult {
		[DataMember]
		public long ClientTimeSent {  get; set; }
		[DataMember]
		public long ServerTimeReceived {  get; set; }

		public RoTimeSync( long clientTime, long serverTime ) {
			ClientTimeSent = clientTime;
			ServerTimeReceived = serverTime;

			Success = true;
		}
	}
}
