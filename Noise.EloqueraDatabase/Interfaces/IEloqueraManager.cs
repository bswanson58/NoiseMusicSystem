using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.Interfaces {
	public interface IEloqueraManager : IDatabaseManager {
		IDatabaseShell	CreateDatabase();

		IDatabase		ReserveDatabase();
		IDatabase		GetDatabase( string databaseId );

		void			FreeDatabase( string databaseId );
		void			FreeDatabase( IDatabase database );

		int				ReservedDatabaseCount { get; }
	}
}
