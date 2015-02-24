using System;
using System.Runtime.CompilerServices;
using Noise.Infrastructure.Dto;

namespace Noise.EntityFrameworkDatabase.Logging {
	internal interface ILogDatabase {
		void	CreatedDatabase( LibraryConfiguration library );
		void	OpenedDatabase( LibraryConfiguration library, DbVersion databaseVersion );
		void	ClosedDatabase();

		void	AddingItem( DbBase item );
		void	AddingExistingItem( DbBase item );
		void	RemoveItem( DbBase item );

		void	LogException( string message, Exception exception, [CallerMemberName] string callerName = "" );
	}
}
