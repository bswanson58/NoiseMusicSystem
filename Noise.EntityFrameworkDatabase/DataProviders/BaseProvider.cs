using CuttingEdge.Conditions;
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
				retValue = GetItemByKey( context, key );
			}

			return( retValue );
		}

		protected TEntity GetItemByKey( IDbContext context, long key ) {
			Condition.Requires( context ).IsNotNull();

			return( context.Set<TEntity>().Find( key ));
		}
	}
}
