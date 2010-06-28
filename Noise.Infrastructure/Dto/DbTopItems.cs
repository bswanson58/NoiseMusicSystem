using System;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public class DbTopItems : ExpiringContent {
		public	long		AssociatedItem { get; private set; }
		public	string[]	TopItems { get; set; }

		public DbTopItems( long associatedItem ) {
			AssociatedItem = associatedItem;
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbTopItems )); }
		}
	}
}
