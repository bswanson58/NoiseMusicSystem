using System;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public class DbTopItems : ExpiringContent {
		public	string[]	TopItems { get; set; }

		public DbTopItems( long associatedItem, ContentType contentType ) :
			base( associatedItem, contentType ) {
			TopItems = new string[0];
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbTopItems )); }
		}
	}
}
