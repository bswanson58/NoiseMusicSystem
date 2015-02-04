using System;
using System.Runtime.CompilerServices;
using Noise.Infrastructure.Dto;

namespace Noise.Core.Logging {
	public interface ILogLibraryConfiguration {
		void	LibraryOpened( LibraryConfiguration configuration );
		void	LibraryClosed( LibraryConfiguration configuration );

		void	LogException( string message, Exception exception, [CallerMemberName] string callerName = "" );
	}
}
