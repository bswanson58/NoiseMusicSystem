using System.Data.Entity;
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

		protected IDbSet<TEntity> Set( IDbContext fromContext ) {
			Condition.Requires( fromContext ).IsNotNull();

			return( fromContext.Set<TEntity>());
		}

		protected void AddItem( TEntity item ) {
			Condition.Requires( item ).IsNotNull();

			using( var context = CreateContext()) {
				if( GetItemByKey( context, item.DbId ) == null ) {
					context.Set<TEntity>().Add( item );
					context.SaveChanges();
				}
			}
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
