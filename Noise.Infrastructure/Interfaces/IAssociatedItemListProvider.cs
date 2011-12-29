using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IAssociatedItemListProvider {
		void									AddAssociationList( DbAssociatedItemList associationList );

		DbAssociatedItemList					GetAssociatedItems( long artistId, ContentType ofType );

		DataProviderList<DbAssociatedItemList>	GetAssociatedItemLists( ContentType forType );
		DataProviderList<DbAssociatedItemList>	GetAssociatedItemLists( long forArtist );

		DataUpdateShell<DbAssociatedItemList>	GetAssociationForUpdate( long listId );
	}
}
