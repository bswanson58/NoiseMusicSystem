using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IDatabaseUtility {
		IEnumerable<string>				GetDatabaseList();
		string							GetDatabaseName( string libraryName );

		void							DetachDatabase( string databaseName );
		void							BackupDatabase( string databaseName, string backupLocation );
		IEnumerable<DatabaseFileInfo>	RestoreFileList( string backupFile );
		void							RestoreDatabase( string databaseName, string restoreLocation );
	}
}
