using System.Data.Entity;
using Noise.EntityFrameworkDatabase.Interfaces;

namespace Noise.EntityFrameworkDatabase.DatabaseManager {
	public class DebugDatabaseInitialize : IDatabaseInitializeStrategy {
		internal class DatabaseCreatedInitializer : DropCreateDatabaseIfModelChanges<NoiseContext> {
			public bool		SeedWasCalled { get; private set; }

			protected override void Seed( NoiseContext context ) {
				SeedWasCalled = true;
			}
		}

		private bool	mDatabaseWasCreated;
        public	bool	DidCreateDatabase => ( mDatabaseWasCreated );

		public bool InitializeDatabase( IDbContext context ) {
			var initializer = new DatabaseCreatedInitializer();

			// swap the next two lines to create a database with a new model.
//			Database.SetInitializer( initializer );	
            Database.SetInitializer<NoiseContext>( null );
			context.Database.Initialize( true );

			mDatabaseWasCreated = initializer.SeedWasCalled;

			return( true );
		}
    }
}
