﻿using System;
using System.Collections.Generic;

namespace Noise.Infrastructure.Dto {
	public class DataProviderBase : IDisposable {
		protected	readonly string		mClientName;
		private		Action<string>		mDisposeAction;

		protected DataProviderBase( string clientName, Action<string> disposeAction ) {
			mClientName = clientName;
			mDisposeAction = disposeAction;
		}

		public void Dispose() {
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
	}

	public class DataUpdateShell<T> : DataProviderBase {
		public	T					Item { get; private set; }
		private readonly Action<string, T>	mOnUpdate;

		public DataUpdateShell( string clientName, Action<string> disposeAction, Action<string, T> onUpdate, T item ) :
			base( clientName, disposeAction ) {
			Item = item;

			mOnUpdate = onUpdate;
		}

		public void Update() {
			if( mOnUpdate != null ) {
				mOnUpdate( mClientName, Item );
			}
		}
	}
}
