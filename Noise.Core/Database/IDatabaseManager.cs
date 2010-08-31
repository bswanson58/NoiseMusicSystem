namespace Noise.Core.Database {
	public interface IDatabaseManager {
		bool		Initialize();
		void		Shutdown();

		IDatabase	ReserveDatabase( string clientName );
		void		FreeDatabase( string databaseId );
	}
}
