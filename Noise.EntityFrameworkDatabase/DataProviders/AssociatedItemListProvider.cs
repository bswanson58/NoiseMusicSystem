using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class AssociatedItemListProvider : BaseProvider<DbAssociatedItemList>, IAssociatedItemListProvider {
		public AssociatedItemListProvider( IContextProvider contextProvider ) :
			base( contextProvider ) { }

		public void AddAssociationList( DbAssociatedItemList associationList ) {
			Condition.Requires( associationList ).IsNotNull();

			AddItem( associationList );
		}

		public DbAssociatedItemList GetAssociatedItems( long artistId, ContentType ofType ) {
			DbAssociatedItemList	retValue;

			using( var context = CreateContext()) {
				retValue = Set( context ).Where( entry => (( entry.Artist == artistId ) && ( entry.DbContentType == (int)ofType )))
											.Include( entry => entry.Items ).FirstOrDefault();
			}

			return( retValue );
		}

		public IDataProviderList<DbAssociatedItemList> GetAssociatedItemLists( ContentType forType ) {
			var context = CreateContext();

			return( new EfProviderList<DbAssociatedItemList>( context, Set( context ).Where( entry => entry.DbContentType == (int)forType )
																						.Include( entry => entry.Items )));
		}

		public IDataProviderList<DbAssociatedItemList> GetAssociatedItemLists( long forArtist ) {
			var context = CreateContext();

			return( new EfProviderList<DbAssociatedItemList>( context, Set( context ).Where( entry => entry.Artist == forArtist )
																						.Include( entry => entry.Items )));
		}

		public IDataUpdateShell<DbAssociatedItemList> GetAssociationForUpdate( long listId ) {
			var context = CreateContext();
			var item = Set( context ).Where( entry => entry.DbId == listId ).Include( entry => entry.Items ).FirstOrDefault();

			return( new AssociatedListUpdateShell( context, item ));
		}
	}

	internal class AssociatedListUpdateShell : EfUpdateShell<DbAssociatedItemList> {
		private readonly List<DbAssociatedItem>	mOriginalItems;
 
		internal AssociatedListUpdateShell( IDbContext context, DbAssociatedItemList list ) :
			base( context, list ) {
			mOriginalItems = new List<DbAssociatedItem>( list.Items );
		}

		public override void Update() {
			var itemSet = mContext.Set<DbAssociatedItem>();

			foreach( var item in mOriginalItems ) {
				if( Item.Items.FirstOrDefault( entry => entry.DbId == item.DbId ) == null ) {
					itemSet.Remove( item );
				}
			}

			base.Update();
		}
	}
}
