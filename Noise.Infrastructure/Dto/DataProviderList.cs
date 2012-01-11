using System;
using System.Collections.Generic;
using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure.Dto {
	public class DataProviderBase : IDisposable {
		protected	readonly IDatabaseShell	mDatabaseShell;

		protected DataProviderBase( IDatabaseShell databaseShell ) {
			mDatabaseShell = databaseShell;
		}

		public void Dispose() {
			if( mDatabaseShell != null ) {
				mDatabaseShell.FreeDatabase();
			}
		}
	}

	public class DataProviderList<T> : DataProviderBase {
		public IEnumerable<T>	List { get; private set; }

		public DataProviderList( IDatabaseShell database, IEnumerable<T> list ) :
			base( database ) {
			List = list;
		}
	}

	public class DataUpdateShell<T> : DataProviderBase {
		public	T					Item { get; private set; }
		private readonly Action<string, T>	mOnUpdate;

		public DataUpdateShell( IDatabaseShell databaseShell, T item ) :
			base( databaseShell ) {
			Item = item;
		}

		public virtual void Update() {
			if( mDatabaseShell != null ) {
				mDatabaseShell.Database.UpdateItem( Item );
			}
		}
	}
}
