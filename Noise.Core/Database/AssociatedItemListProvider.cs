using System.Collections.Generic;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class AssociatedItemListProvider : BaseDataProvider<DbAssociatedItemList>, IAssociatedItemListProvider {
		public AssociatedItemListProvider( IDatabaseManager databaseManager ) :
			base( databaseManager ) { }

		public void AddAssociationList( DbAssociatedItemList associationList ) {
			Condition.Requires( associationList ).IsNotNull();

			using( var dbShell = CreateDatabase()) {
				dbShell.InsertItem( associationList );
			}
		}

		public DbAssociatedItemList GetAssociatedItems( long artistId, ContentType ofType ) {
			return( TryGetItem( "SELECT DbAssociatedItemList Where Artist = @artistId AND ContentType = @contentType",
								new Dictionary<string, object> {{ "artistId", artistId }, { "contentType", ofType }}, "GetAssociatedItems" ));
		}

		public DataProviderList<DbAssociatedItemList> GetAssociatedItemLists( ContentType forType ) {
			return( TryGetList( "SELECT DbAssociatedItemList ContentType = @contentType", new Dictionary<string, object> {{ "contentType", forType }}, "GetAssociatedItemList" ));
		}

		public DataProviderList<DbAssociatedItemList> GetAssociatedItemLists( long forArtist ) {
			return( TryGetList( "SELECT DbAssociatedItemList Where Artist = @artistId", new Dictionary<string, object> {{ "artistId", forArtist }}, "GetAssociatedItemLists" ));
		}

		public DataUpdateShell<DbAssociatedItemList> GetAssociationForUpdate( long listId ) {
			return( GetUpdateShell( "SELECT DbAssociatedItemList Where DbId = @listId", new Dictionary<string, object> {{ "listId", listId }}));
		}
	}
}
