namespace Noise.Core.MetaData {
	public class DbSimilarItems : ExpiringContent {
		public	long		AssociatedItem { get; private set; }
		public	string[]	SimilarItems { get; set; }

		public DbSimilarItems( long associatedItem ) {
			AssociatedItem = associatedItem;
		}
	}
}
