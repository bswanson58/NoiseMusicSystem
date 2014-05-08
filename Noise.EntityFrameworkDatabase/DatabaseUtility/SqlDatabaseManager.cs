using System.Collections.Generic;
using System.Data.SqlClient;
using Noise.EntityFrameworkDatabase.DatabaseManager;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DatabaseUtility {
	public class DatabaseFileInfo {
		public	string		LogicalName { get; private set; }
		public	string		PhysicalName { get; private set; }
		public	string		FileType { get; private set; }
		public	long		FileSize { get; private set; }

		public DatabaseFileInfo( string logicalName, string physicalName, string fileType, long fileSize ) {
			LogicalName = logicalName;
			PhysicalName = physicalName;
			FileType = fileType;
			FileSize = fileSize;
		}
	}

	public class SqlDatabaseManager : IDatabaseUtility {

		public IEnumerable<string> GetDatabaseList() {
			var retValue = new List<string>();

			using( var connection = CreateConnection()) {
				connection.Open();
				const string cmdText = "SELECT DB_NAME(database_id), name, physical_name FROM sys.master_files WHERE type = 0 AND database_id > 4;";

				using( var command = new SqlCommand( cmdText, connection )) {
					using( var reader = command.ExecuteReader()) {
						while( reader.Read()) {
							retValue.Add( reader.GetString( 1 ));
						}
					}
				}
			}

			return( retValue );
		}

		public string GetDatabaseName( string libraryName ) {
			var retValue = string.Empty;
			var databaseName = ContextProvider.FormatDatabaseName( libraryName );

			using( var connection = CreateConnection()) {
				connection.Open();
				var cmdText = string.Format( "SELECT DB_NAME(database_id), name, physical_name FROM sys.master_files WHERE name = '{0}';", databaseName );

				using( var command = new SqlCommand( cmdText, connection )) {
					using( var reader = command.ExecuteReader()) {
						while( reader.Read()) {
							retValue = reader.GetString( 0 );
						}
					}
				}
			}

			return( retValue );
		}

		public string GetDatabaseLocation( string databaseName ) {
			var retValue = string.Empty;

			using( var connection = CreateConnection()) {
				connection.Open();
				var cmdText = string.Format( "SELECT DB_NAME(database_id), name, physical_name FROM sys.master_files WHERE name = '{0}';", databaseName );

				using( var command = new SqlCommand( cmdText, connection )) {
					using( var reader = command.ExecuteReader()) {
						while( reader.Read()) {
							retValue = reader.GetString( 1 );
						}
					}
				}
			}

			return( retValue );
		}


		public void AttachDatabase( string databaseName, string databaseFile ) {
			using( var connection = CreateConnection()) {
				string	commandText = string.Format( "exec sys.sp_attach_db {0} '{1}'", databaseName, databaseFile );

				connection.Open();

				using( var command = new SqlCommand( commandText, connection )) {
					command.ExecuteNonQuery();
				}
			}
		}

		public void DetachDatabase( string databaseName ) {
			using( var connection = CreateConnection()) {
				string	commandText = string.Format( "exec sys.sp_detach_db {0}", databaseName );

				connection.Open();

				using( var command = new SqlCommand( commandText, connection )) {
					command.ExecuteNonQuery();
				}
			}
		}

		public void BackupDatabase( string databaseName, string backupLocation ) {
			using( var connection = CreateConnection()) {
				string	commandText = string.Format( "BACKUP DATABASE {0} TO DISK='{1}' WITH FORMAT, MEDIANAME='NoiseBackup', MEDIADESCRIPTION='Media set for {0} database';",
													databaseName, backupLocation );

				connection.Open();

				using( var command = new SqlCommand( commandText, connection )) {
					command.ExecuteNonQuery();
				}
			}
		}

		public void RestoreDatabase( string databaseName, string restoreLocation ) {
			using( var connection = CreateConnection()) {
				string	commandText = string.Format( "RESTORE DATABASE {0} FROM DISK='{1}' WITH REPLACE, RECOVERY;",
													databaseName, restoreLocation );

				connection.Open();

				using( var command = new SqlCommand( commandText, connection )) {
					command.ExecuteNonQuery();
				}
			}
		}

		public IEnumerable<DatabaseFileInfo> RestoreFileList( string backupFile ) {
			var retValue = new List<DatabaseFileInfo>();

			using( var connection = CreateConnection()) {
				string	commandText = string.Format( "RESTORE FILELISTONLY FROM DISK='{0}'", backupFile );

				connection.Open();

				using( var command = new SqlCommand( commandText, connection )) {
					using( var reader = command.ExecuteReader()) {
						while( reader.Read()) {
							retValue.Add( new DatabaseFileInfo( reader.GetString( 0 ), reader.GetString( 1 ), reader.GetString( 2 ), reader.GetInt64( 4 )));
						}
					}
				}
			}

			return( retValue );
		}

		private SqlConnection CreateConnection() {
			return( new SqlConnection( @"Server=(localdb)\v11.0;Integrated Security=true;" ));
		}
	}
}
