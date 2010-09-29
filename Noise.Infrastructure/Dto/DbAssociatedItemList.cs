using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Eloquera.Client;

namespace Noise.Infrastructure.Dto {
	public class DbAssociatedItem {
		public	string	Item { get; private set; }
		public	long	AssociatedId { get; private set; }

		public DbAssociatedItem( string item ) {
			Item = item;
			AssociatedId = Constants.cDatabaseNullOid;
		}

		public void SetAssociatedId( long dbid ) {
			AssociatedId = dbid;
		}

		[Ignore]
		public bool IsLinked {
			get{ return( AssociatedId != Constants.cDatabaseNullOid ); }
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbAssociatedItem )); }
		}
	}

	public class DbAssociatedItemList : ExpiringContent {
		public	DbAssociatedItem[]	Items { get; set; }

		public DbAssociatedItemList( long associatedItem, ContentType contentType ) :
			base( associatedItem, contentType ) {
			Items = new DbAssociatedItem[0];
		}

		public void SetItems( List<string> items ) {
			if( items != null ) {
				Items = new DbAssociatedItem[items.Count];

				for( var index = 0; index < items.Count; index++ ) {
					Items[index] = new DbAssociatedItem( items[index]);
				}
			} 
		}

		public IEnumerable<string> GetItems() {
			return( Items.Select( item => item.Item ).ToList());
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbAssociatedItemList )); }
		}
	}
}
