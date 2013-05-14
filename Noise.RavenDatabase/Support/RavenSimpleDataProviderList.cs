using System.Collections.Generic;
using Noise.Infrastructure.Interfaces;

namespace Noise.RavenDatabase.Support {
	public class RavenSimpleDataProviderList<T> : IDataProviderList<T> {
		public IEnumerable<T>		List { get; private set; }

		public RavenSimpleDataProviderList( IEnumerable<T> list ) {
			List = list;
		} 

		public void Dispose() {
		}
	}
}
