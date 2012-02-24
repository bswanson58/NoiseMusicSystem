using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public abstract class BaseProvider<TEntity> where TEntity : DbBase {
		private	readonly IContextProvider	mContextProvider;

		protected BaseProvider( IContextProvider contextProvider ) {
			mContextProvider = contextProvider;
		}

		protected IDbContext CreateContext() {
			return( mContextProvider.CreateContext());
		}

		protected TEntity GetItemByKey( long key ) {
			TEntity	retValue;

			using( var context = CreateContext()) {
				retValue = context.Set<TEntity>().Find( key );
			}

			return( retValue );
		}
	}
}
