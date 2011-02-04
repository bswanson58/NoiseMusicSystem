using System;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public class DbTimestamp {
		public	string		ComponentId { get; private set; }
		public	long		Timestamp { get; private set; }


		public DbTimestamp( string componentId ) {
			ComponentId = componentId;

			Timestamp = 0;
		}

		public void UpdateTimestamp() {
			Timestamp = DateTime.Now.Ticks;
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbTimestamp )); }
		}
	}
}
