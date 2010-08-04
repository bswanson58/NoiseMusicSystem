using System;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public class DbSimilarItems : ExpiringContent {
		public	string[]	SimilarItems { get; set; }

		public DbSimilarItems( long associatedItem, ContentType contentType ) :
			base( associatedItem, contentType ) {
			SimilarItems = new string[0];
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbSimilarItems )); }
		}
	}
}
