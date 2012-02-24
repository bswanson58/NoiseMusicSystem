using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IAssociatedItemListProvider {
		void									AddAssociationList( DbAssociatedItemList associationList );

		DbAssociatedItemList					GetAssociatedItems( long artistId, ContentType ofType );

		IDataProviderList<DbAssociatedItemList>	GetAssociatedItemLists( ContentType forType );
		IDataProviderList<DbAssociatedItemList>	GetAssociatedItemLists( long forArtist );
		
		IDataUpdateShell<DbAssociatedItemList>	GetAssociationForUpdate( long listId );
	}
}
