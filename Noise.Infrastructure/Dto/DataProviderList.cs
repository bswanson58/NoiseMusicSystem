using System;
using System.Collections.Generic;

namespace Noise.Infrastructure.Dto {
	public class DataProviderBase : IDisposable {
		private readonly string			mClientName;
		private readonly Action<string>	mDisposeAcion;

		protected DataProviderBase( string clientName, Action<string> disposeAction ) {
			mClientName = clientName;
			mDisposeAcion = disposeAction;
		}

		public void Dispose() {
			if( mDisposeAcion != null ) {
				mDisposeAcion( mClientName );
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
}
