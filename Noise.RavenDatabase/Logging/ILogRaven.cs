using System;
using System.Runtime.CompilerServices;
using Noise.Infrastructure.Dto;

namespace Noise.RavenDatabase.Logging {
	internal interface ILogRaven {
		void	CreatedDatabase( LibraryConfiguration library );
		void	OpenedDatabase( LibraryConfiguration library );
		void	ClosedDatabase();

		void	AddingItem( object item );
		void	AddingExistingItem( object item );
		void	UpdateUnknownItem( object item );
		void	RemoveItem( object item );
		void	RemoveUnknownItem( object item );

		void	LogException( string message, Exception exception, [CallerMemberName] string callerName = "" );
	}
}
