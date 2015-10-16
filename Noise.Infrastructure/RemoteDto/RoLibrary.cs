using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoLibrary {
		[DataMember]
		public	long		LibraryId { get; set; }
		[DataMember]
		public	string		LibraryName { get; set; }
		[DataMember]
		public	string		DatabaseName { get; set; }
		[DataMember]
		public	bool		IsDefaultLibrary { get; set; }
		[DataMember]
		public	string		MediaLocation { get; set; }
	}
}
