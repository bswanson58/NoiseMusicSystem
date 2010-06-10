namespace Noise.Core {
	public interface IDatabaseManager {
		bool	InitializeDatabase( string databaseLocation );
		void	OpenWithCreateDatabase( string databaseName );
		bool	OpenDatabase( string databaseName );
	}
}
