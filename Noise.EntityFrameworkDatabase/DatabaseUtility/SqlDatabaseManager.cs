using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Practices.ObjectBuilder2;
using Noise.EntityFrameworkDatabase.DatabaseManager;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DatabaseUtility {
	public class SqlDatabaseManager : IDatabaseUtility {

		public IEnumerable<DatabaseInfo> GetDatabaseList() {
			var retValue = new List<DatabaseInfo>();

			using( var connection = CreateConnection()) {
				connection.Open();
				const string cmdText = "SELECT DB_NAME(database_id), name, physical_name FROM sys.master_files WHERE type = 0 AND database_id > 4;";

				using( var command = new SqlCommand( cmdText, connection )) {
					using( var reader = command.ExecuteReader()) {
						while( reader.Read()) {
							retValue.Add( new DatabaseInfo( reader.GetString( 0 ), reader.GetString( 1 )));
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

		public void DetachDatabase( DatabaseInfo database ) {
			using( var connection = CreateConnection()) {
				string	commandText = string.Format( "exec sys.sp_detach_db {0}", database.DatabaseName );

				connection.Open();

				using( var command = new SqlCommand( commandText, connection )) {
					command.ExecuteNonQuery();
				}
			}
		}

		public void BackupDatabase( string databaseName, string backupLocation ) {
			using( var connection = CreateConnection()) {
				string	commandText = string.Format( "BACKUP DATABASE [{0}] TO DISK='{1}' WITH FORMAT, MEDIANAME='NoiseBackup', MEDIADESCRIPTION='Media set for {0} database';",
													databaseName, backupLocation );

				connection.Open();

				using( var command = new SqlCommand( commandText, connection )) {
					command.ExecuteNonQuery();
				}
			}
		}

		public void RestoreDatabase( string databaseName, string restoreLocation ) {
			using( var connection = CreateConnection()) {
				string	commandText = string.Format( "RESTORE DATABASE [{0}] FROM DISK='{1}' WITH REPLACE, RECOVERY;",
													databaseName, restoreLocation );

				connection.Open();

				using( var command = new SqlCommand( commandText, connection )) {
					command.ExecuteNonQuery();
				}
			}
		}

		public void RestoreDatabase( string databaseName, string restoreLocation, IEnumerable<string> fileList, IEnumerable<string> locationList ) {
			using( var connection = CreateConnection()) {
				var moveList = new List<string>();

				for( int index = 0; index < fileList.Count(); index++ ) {
					moveList.Add( string.Format( "MOVE '{0}' TO '{1}'", fileList.ElementAt( index ), locationList.ElementAt( index )));
				}

				var		moveText = moveList.JoinStrings( ", " );
				string	commandText = string.Format( "RESTORE DATABASE [{0}] FROM DISK='{1}' WITH REPLACE, RECOVERY, {2}",
													databaseName, restoreLocation, moveText );

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
