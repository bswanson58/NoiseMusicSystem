using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.EntityFrameworkDatabase.Logging;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	internal abstract class BaseProvider<TEntity> where TEntity : DbBase {
		private	readonly IContextProvider	mContextProvider;
		private readonly ILogDatabase		mLog;

		protected BaseProvider( IContextProvider contextProvider, ILogDatabase log ) {
			mContextProvider = contextProvider;
			mLog = log;
		}

		protected ILogDatabase Log {
			get {  return( mLog ); }
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
				if( context.IsValidContext ) {
#if DEBUG
					if( GetItemByKey( context, item.DbId ) == null ) {
#endif
						context.Set<TEntity>().Add( item );

						context.SaveChanges();

						Log.AddingItem( item );
#if DEBUG
					}
					else {
						Log.AddingExistingItem( item );
					}
#endif
				}
			}
		}

		protected void AddList( IEnumerable<TEntity> list ) {
			using( var context = CreateContext()) {
				if( context.IsValidContext ) {
					foreach( var item in list ) {
						context.Set<TEntity>().Add( item );

						Log.AddingItem( item );
					}

					context.SaveChanges();
				}
			}
		}

		protected void RemoveItem( TEntity item ) {
			Condition.Requires( item ).IsNotNull();

			using( var context = CreateContext()) {
				if( context.IsValidContext ) {
					var entry = GetItemByKey( context, item.DbId );

					if( entry != null ) {
						Set( context ).Remove( entry );

						context.SaveChanges();

						Log.RemoveItem( item );
					}
				}
			}
		}

		protected void RemoveItem( long itemId ) {
			using( var context = CreateContext()) {
				if( context.IsValidContext ) {
					var item = GetItemByKey( context, itemId );

					if( item != null ) {
						Set( context ).Remove( item );

						context.SaveChanges();

						Log.RemoveItem( item );
					}
				}
			}
		}

		protected TEntity GetItemByKey( long key ) {
			TEntity	retValue = default( TEntity );

			using( var context = CreateContext()) {
				if( context.IsValidContext ) {
					retValue = GetItemByKey( context, key );
				}
			}

			return( retValue );
		}

		protected TEntity GetItemByKey( IDbContext context, long key ) {
			Condition.Requires( context ).IsNotNull();
			Condition.Requires( context.IsValidContext );

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
