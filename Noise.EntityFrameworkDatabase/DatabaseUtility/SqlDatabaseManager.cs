using System.Collections.Generic;
using System.Data.SqlClient;

namespace Noise.EntityFrameworkDatabase.DatabaseUtility {
	public class SqlDatabaseManager {

		public IEnumerable<string> GetDatabaseList() {
			var retValue = new List<string>();

			using( var connection = CreateConnection()) {
				connection.Open();

				using( var command = new SqlCommand( "SELECT [name] FROM sys.databases", connection )) {
					using( var reader = command.ExecuteReader()) {
						while( reader.Read() ) {
							retValue.Add( reader.GetString( 0 ));
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

		private SqlConnection CreateConnection() {
			return( new SqlConnection( @"Server=(localdb)\v11.0;Integrated Security=true;" ));
		}
	}
}
