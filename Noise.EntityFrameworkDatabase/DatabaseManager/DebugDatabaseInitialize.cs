using System.Data.Entity;
using Noise.EntityFrameworkDatabase.Interfaces;

namespace Noise.EntityFrameworkDatabase.DatabaseManager {
	public class DebugDatabaseInitialize : IDatabaseInitializeStrategy {
		public bool InitializeDatabase( IDbContext context ) {
			Database.SetInitializer( new DropCreateDatabaseIfModelChanges<NoiseContext>());	
			context.Database.Initialize( true );

			return( true );
		}
	}
}
