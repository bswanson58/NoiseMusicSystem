using System;
using System.Runtime.CompilerServices;
using Noise.Infrastructure.Dto;

namespace Noise.RavenDatabase.Logging {
	internal interface ILogRaven {
		void	CreatedDatabase( LibraryConfiguration library );
		void	OpenedDatabase( LibraryConfiguration library );
		void	ClosedDatabase();

		void	AddingItem( DbBase item );
		void	AddingExistingItem( DbBase item );
		void	RemoveItem( DbBase item );

		void	LogException( string message, Exception exception, [CallerMemberName] string callerName = "" );
	}
}
