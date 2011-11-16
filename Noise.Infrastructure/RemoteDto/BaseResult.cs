using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class BaseResult {
		[DataMember]
		public bool		Success { get; set; }
		[DataMember]
		public string	ErrorMessage { get; set; }

		public BaseResult() {
			ErrorMessage = "";
		}
	}
}
