using Noise.EntityFrameworkDatabase.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	public class TestDatabaseInitializer : IDatabaseInitializeStrategy {
		public bool InitializeDatabase( IDbContext context ) {
			context.Database.Delete();
			context.Database.Create();

			return( true );
		}

		public bool DidCreateDatabase {
			get { return( true ); }
		}
	}
}
