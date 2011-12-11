namespace Noise.Infrastructure.Interfaces {
	public interface IDatabaseManager {
		bool			Initialize();
		void			Shutdown();

		IDatabase		ReserveDatabase();
		IDatabase		GetDatabase( string databaseId );

		void			FreeDatabase( string databaseId );
		void			FreeDatabase( IDatabase database );
	}
}
