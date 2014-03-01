using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class LibraryListResult : BaseResult {
		[DataMember]
		public	RoLibrary[]		Libraries { get; set; }

		public LibraryListResult() {
			Libraries = new RoLibrary[0];
		}
	}
}
