using System;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public class DbSimilarItems : ExpiringContent {
		public	long		AssociatedItem { get; private set; }
		public	string[]	SimilarItems { get; set; }

		public DbSimilarItems( long associatedItem ) {
			AssociatedItem = associatedItem;
			SimilarItems = new string[0];
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbSimilarItems )); }
		}
	}
}
