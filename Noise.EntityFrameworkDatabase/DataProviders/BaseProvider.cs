using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public abstract class BaseProvider<TEntity> where TEntity : DbBase {
		private	readonly IContextProvider	mContextProvider;

		protected BaseProvider( IContextProvider contextProvider ) {
			mContextProvider = contextProvider;
		}

		protected IDbContext CreateContext() {
			return( mContextProvider.CreateContext());
		}

		protected IBlobStorage BlobStorage {
			get{ return( mContextProvider.BlobStorageManager.GetStorage()); }
		}

		protected IDbSet<TEntity> Set( IDbContext fromContext ) {
			Condition.Requires( fromContext ).IsNotNull();

			return( fromContext.Set<TEntity>());
		}

		protected void AddItem( TEntity item ) {
			Condition.Requires( item ).IsNotNull();

			using( var context = CreateContext()) {
#if DEBUG
				if( GetItemByKey( context, item.DbId ) == null ) {
#endif
					context.Set<TEntity>().Add( item );

					context.SaveChanges();
#if DEBUG
				}
				else {
					NoiseLogger.Current.LogMessage( "Attempting to add an existing item: {0}", item.ToString());
				}
#endif
			}
		}

		protected void AddList( IEnumerable<TEntity> list ) {
			using( var context = CreateContext()) {
				foreach( var item in list ) {
					context.Set<TEntity>().Add( item );
				}

				context.SaveChanges();
			}
		}

		protected void RemoveItem( TEntity item ) {
			Condition.Requires( item ).IsNotNull();

			using( var context = CreateContext()) {
				var entry = GetItemByKey( context, item.DbId );

				if( entry != null ) {
					Set( context ).Remove( entry );

					context.SaveChanges();
				}
			}
		}

		protected void RemoveItem( long itemId ) {
			using( var context = CreateContext()) {
				var item = GetItemByKey( context, itemId );

				if( item != null ) {
					Set( context ).Remove( item );

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

		protected EfProviderList<TEntity> GetListShell() {
			var context = CreateContext();

			return( new EfProviderList<TEntity>( context, Set( context )));
		} 

		protected EfUpdateShell<TEntity> GetUpdateShell( long forItem ) {
			var context = CreateContext();

			return( new EfUpdateShell<TEntity>( context, GetItemByKey( context, forItem )));
		} 

		protected long GetEntityCount() {
			return( CreateContext().Set<TEntity>().Count());
		}
	}
}
