using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public class DatabaseInfo {
		public string	DatabaseName { get; private set; }
		public string	PhysicalName { get; private set; }

		public DatabaseInfo( string databasename, string physicalName ) {
			DatabaseName = databasename;
			PhysicalName = physicalName;
		}

		public DatabaseInfo( string databaseName ) {
			DatabaseName = databaseName;
			PhysicalName = string.Empty;
		}
	}

	public interface IDatabaseUtility {
		IEnumerable<DatabaseInfo>		GetDatabaseList();
		string							GetDatabaseName( string libraryName );

		void							DetachDatabase( DatabaseInfo database );
		void							BackupDatabase( string databaseName, string backupLocation );
		IEnumerable<DatabaseFileInfo>	RestoreFileList( string backupFile );
		void							RestoreDatabase( string databaseName, string restoreLocation );
		void							RestoreDatabase( string databaseName, string restoreLocation, IEnumerable<string> fileList, IEnumerable<string> locationList );
	}
}
