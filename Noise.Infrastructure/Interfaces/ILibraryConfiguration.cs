using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ILibraryConfiguration {
		LibraryConfiguration				Current { get; }
		IEnumerable<LibraryConfiguration>	Libraries { get; }

		void		Open( long libraryId );
		void		Open( LibraryConfiguration configuration );
		void		Close( LibraryConfiguration configuration );
		void		AddLibrary( LibraryConfiguration configuration );
		void		UpdateLibrary( LibraryConfiguration configuration );
		void		DeleteLibrary( LibraryConfiguration configuration );
	}
}
