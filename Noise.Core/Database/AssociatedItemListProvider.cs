using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class AssociatedItemListProvider : BaseDataProvider<DbAssociatedItemList>, IAssociatedItemListProvider {
		public AssociatedItemListProvider( IDatabaseManager databaseManager ) :
			base( databaseManager ) { }

		public DbAssociatedItemList GetAssociatedItems( long artistId, ContentType ofType ) {
			return( TryGetItem( "SELECT DbAssociatedItemList Where Artist = @artistId AND ContentType = @contentType",
								new Dictionary<string, object> {{ "artistId", artistId }, { "contentType", ofType }}, "Exception - GetAssociatedItems" ));
		}

		public DataProviderList<DbAssociatedItemList> GetAssociatedItemLists( ContentType forType ) {
			return( TryGetList( "SELECT DbAssociatedItemList ContentType = @contentType", new Dictionary<string, object> {{ "contentType", forType }}, "GetAssociatedItemList" ));
		}

		public DataUpdateShell<DbAssociatedItemList> GetAssociationForUpdate( long listId ) {
			return( GetUpdateShell( "SELECT DbAssociatedItemList Where DbId = @listId", new Dictionary<string, object> {{ "listId", listId }}));
		}
	}
}
