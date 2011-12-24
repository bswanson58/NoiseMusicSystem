using System;
using System.Collections.Generic;
using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure.Dto {
	public class DataProviderBase : IDisposable {
		protected	readonly IDatabaseShell	mDatabaseShell;
		protected	readonly string			mClientName;
		private		Action<string>			mDisposeAction;

		protected DataProviderBase( string clientName, Action<string> disposeAction ) {
			mClientName = clientName;
			mDisposeAction = disposeAction;
		}

		protected DataProviderBase( IDatabaseShell databaseShell ) {
			mDatabaseShell = databaseShell;
			mClientName = "";
		}

		public void Dispose() {
			if( mDatabaseShell != null ) {
				mDatabaseShell.FreeDatabase();
			}

			if( mDisposeAction != null ) {
				mDisposeAction( mClientName );

				mDisposeAction = null;
			}
		}
	}

	public class DataProviderList<T> : DataProviderBase {
		public IEnumerable<T>	List { get; private set; }

		public DataProviderList( string clientName, Action<string> disposeAction, IEnumerable<T> list ) :
			base( clientName, disposeAction ) {
			List = list;
		}

		public DataProviderList( IDatabaseShell database, IEnumerable<T> list ) :
			base( database ) {
			List = list;
		}
	}

	public class DataUpdateShell<T> : DataProviderBase {
		public	T					Item { get; private set; }
		private readonly Action<string, T>	mOnUpdate;

		public DataUpdateShell( string clientName, Action<string> disposeAction, Action<string, T> onUpdate, T item ) :
			base( clientName, disposeAction ) {
			Item = item;

			mOnUpdate = onUpdate;
		}

		public DataUpdateShell( IDatabaseShell databaseShell, T item ) :
			base( databaseShell ) {
			Item = item;
		}

		public void Update() {
			if( mDatabaseShell != null ) {
				mDatabaseShell.UpdateItem( Item );
			}

			if( mOnUpdate != null ) {
				mOnUpdate( mClientName, Item );
			}
		}
	}
}
