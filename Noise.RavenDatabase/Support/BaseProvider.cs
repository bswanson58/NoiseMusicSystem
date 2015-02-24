using System;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Logging;

namespace Noise.RavenDatabase.Support {
	internal class BaseProvider<T> where T : class {
		private readonly IDbFactory			mDbFactory;
		private readonly ILogRaven			mLog;
		private readonly Func<T, object[]>	mKeySelector; 
		private	IRepository<T>				mDatabase;

		protected BaseProvider( IDbFactory factory, Func<T, object[]> keySelector, ILogRaven log ) {
			mDbFactory = factory;
			mLog = log;
			mKeySelector = keySelector;

			mDbFactory.DatabaseClosed.Subscribe( OnDatabaseClosed );
		} 

		private void OnDatabaseClosed( bool isClosed ) {
			mDatabase = null;
		}

		protected IRepository<T> Database {
			get {
				if( mDatabase == null ) {
					mDatabase = new RavenRepository<T>( mDbFactory.GetLibraryDatabase(), mKeySelector, mLog );
				}

				return( mDatabase );
			}
		}
 
		protected IDbFactory DbFactory {
			get{ return( mDbFactory ); }
		}

		protected ILogRaven Log {
			get {  return( mLog ); }
		}
	}
}
