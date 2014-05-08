using System.Collections.Generic;

namespace Noise.Infrastructure.Interfaces {
	public interface IDatabaseUtility {
		IEnumerable<string> GetDatabaseList();
		string				GetDatabaseName( string libraryName );

		void				BackupDatabase( string databaseName, string backupLocation );
		void				RestoreDatabase( string databaseName, string restoreLocation );
	}
}
