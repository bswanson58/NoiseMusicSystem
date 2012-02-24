using System.Collections.Generic;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase {
	public class EloqueraProviderList<T> : EloqueraProviderBase, IDataProviderList<T> {
		public IEnumerable<T>	List { get; private set; }

		public EloqueraProviderList( IDatabaseShell database, IEnumerable<T> list ) :
			base( database ) {
			List = list;
		}
	}
}
