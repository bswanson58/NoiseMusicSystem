using System;
using System.Runtime.Serialization;
using Noise.Infrastructure.Dto;

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

		public RoLibrary( LibraryConfiguration library ) {
			LibraryId = library.LibraryId;
			LibraryName = library.LibraryName;
			DatabaseName = library.DatabaseName;
			IsDefaultLibrary = library.IsDefaultLibrary;
			MediaLocation = library.MediaLocations.Count > 0 ? library.MediaLocations[0].Path : String.Empty;
        }
	}
}
