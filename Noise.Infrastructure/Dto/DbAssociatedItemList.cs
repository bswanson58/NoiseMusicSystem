using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Eloquera.Client;

namespace Noise.Infrastructure.Dto {
	public class DbAssociatedItem : DbBase {
		public	string	Item { get; private set; }
		public	long	AssociatedId { get; private set; }

		public DbAssociatedItem() :
			this( string.Empty ) { }

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
		public	List<DbAssociatedItem>	Items { get; protected set; }

		public DbAssociatedItemList() :
			this( Constants.cDatabaseNullOid, ContentType.Unknown ) {
			Items = new List<DbAssociatedItem>();
		}

		public DbAssociatedItemList( long associatedItem, ContentType contentType ) :
			base( associatedItem, contentType ) {
			Items = new List<DbAssociatedItem>();
		}

		public void SetItems( List<string> items ) {
			if( items != null ) {
				Items.Clear();

				foreach( string t in items ) {
					Items.Add( new DbAssociatedItem( t ));
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
