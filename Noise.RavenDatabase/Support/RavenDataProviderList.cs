using System.Collections.Generic;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;

namespace Noise.RavenDatabase.Support {
	public class RavenDataProviderList<T> : IDataProviderList<T> where T : class {
		private IQuerySession<T>	mSession;
 
		public IEnumerable<T>		List { get; private set; }

		public RavenDataProviderList( IQuerySession<T> querySession ) {
			mSession = querySession;

			List = mSession.Query();
		}

		public RavenDataProviderList( IEnumerable<T> list ) {
			List = list;
		} 

		public void Dispose() {
			if( mSession != null ) {
				mSession.Dispose();

				mSession = null;
			}
		}
	}
}
