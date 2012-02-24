using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DatabaseManager {
	public class EntityFrameworkDatabaseManager : IDatabaseManager {
		private readonly IDatabaseInitializeStrategy	mInitializeStrategy;
		private readonly IContextProvider				mContextProvider;

		public EntityFrameworkDatabaseManager( IDatabaseInitializeStrategy initializeStrategy, IContextProvider contextProvider ) {
			mInitializeStrategy = initializeStrategy;
			mContextProvider = contextProvider;
		}

		public bool Initialize() {
			var retValue = false;

			if( mInitializeStrategy != null ) {
				retValue = mInitializeStrategy.InitializeDatabase( mContextProvider.CreateContext());
			}

			return( retValue );
		}

		public void Shutdown() {
		}
	}
}
