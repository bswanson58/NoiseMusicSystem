using System.Collections.Generic;
using CuttingEdge.Conditions;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.DataProviders {
	internal class AssociatedItemListProvider : BaseDataProvider<DbAssociatedItemList>, IAssociatedItemListProvider {
		public AssociatedItemListProvider( IEloqueraManager databaseManager ) :
			base( databaseManager ) { }

		public void AddAssociationList( DbAssociatedItemList associationList ) {
			Condition.Requires( associationList ).IsNotNull();

			InsertItem( associationList );
		}

		public DbAssociatedItemList GetAssociatedItems( long artistId, ContentType ofType ) {
			return( TryGetItem( "SELECT DbAssociatedItemList Where Artist = @artistId AND ContentType = @contentType",
								new Dictionary<string, object> {{ "artistId", artistId }, { "contentType", ofType }}, "GetAssociatedItems" ));
		}

		public IDataProviderList<DbAssociatedItemList> GetAssociatedItemLists( ContentType forType ) {
			return( TryGetList( "SELECT DbAssociatedItemList Where ContentType = @contentType", new Dictionary<string, object> {{ "contentType", forType }}, "GetAssociatedItemList" ));
		}

		public IDataProviderList<DbAssociatedItemList> GetAssociatedItemLists( long forArtist ) {
			return( TryGetList( "SELECT DbAssociatedItemList Where Artist = @artistId", new Dictionary<string, object> {{ "artistId", forArtist }}, "GetAssociatedItemLists" ));
		}

		public IDataUpdateShell<DbAssociatedItemList> GetAssociationForUpdate( long listId ) {
			return( GetUpdateShell( "SELECT DbAssociatedItemList Where DbId = @listId", new Dictionary<string, object> {{ "listId", listId }}));
		}
	}
}
