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
				retValue = ( from list in Set( context ) where (( list.Artist == artistId ) && ( list.DbContentType == (int)ofType )) select list ).FirstOrDefault();
			}

			return( retValue );
		}

		public IDataProviderList<DbAssociatedItemList> GetAssociatedItemLists( ContentType forType ) {
			var context = CreateContext();

			return( new EfProviderList<DbAssociatedItemList>( context, from list in Set( context ) where list.DbContentType == (int)forType select list ));
		}

		public IDataProviderList<DbAssociatedItemList> GetAssociatedItemLists( long forArtist ) {
			var context = CreateContext();

			return( new EfProviderList<DbAssociatedItemList>( context, from list in Set( context ) where list.Artist == forArtist select list ));
		}

		public IDataUpdateShell<DbAssociatedItemList> GetAssociationForUpdate( long listId ) {
			var context = CreateContext();

			return( new EfUpdateShell<DbAssociatedItemList>( context, GetItemByKey( context, listId )));
		}
	}
}
