using System;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public class DbAssociatedItems : ExpiringContent {
		public	string[]	Items { get; set; }

		public DbAssociatedItems( long associatedItem, ContentType contentType ) :
			base( associatedItem, contentType ) {
			Items = new string[0];
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbAssociatedItems )); }
		}
	}
}
