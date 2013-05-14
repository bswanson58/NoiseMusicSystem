using System;
using Noise.RavenDatabase.Interfaces;

namespace Noise.RavenDatabase.Support {
	public class BaseProvider<T> where T : class {
		private readonly IDbFactory			mDbFactory;
		private readonly Func<T, object[]>	mKeySelector; 
		private	IRepository<T>				mDatabase;

		protected BaseProvider( IDbFactory factory, Func<T, object[]> keySelector ) {
			mDbFactory = factory;
			mKeySelector = keySelector;

			mDbFactory.DatabaseClosed.Subscribe( OnDatabaseClosed );
		} 

		private void OnDatabaseClosed( bool isClosed ) {
			mDatabase = null;
		}

		protected IRepository<T> Database {
			get {
				if( mDatabase == null ) {
					mDatabase = new RavenRepository<T>( mDbFactory.GetLibraryDatabase(), mKeySelector );
				}

				return( mDatabase );
			}
		}
 
		protected IDbFactory DbFactory {
			get{ return( mDbFactory ); }
		}
	}
}
