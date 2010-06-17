namespace Noise.Core.Database {
	public interface IDatabaseManager {
		Eloquera.Client.DB	Database { get; }

		bool		InitializeDatabase( string databaseLocation );
		void		OpenWithCreateDatabase( string databaseName );
		bool		OpenDatabase( string databaseName );
	}
}
