namespace Noise.Core.Database {
	public interface IDatabaseManager {
		bool		Initialize();
		void		Shutdown();

		IDatabase	ReserveDatabase();
		void		FreeDatabase( string databaseId );
		void		FreeDatabase( IDatabase database );
	}
}
