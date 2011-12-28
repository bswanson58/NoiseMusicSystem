using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IAssociatedItemListProvider {
		DbAssociatedItemList					GetAssociatedItems( long artistId, ContentType ofType );

		DataProviderList<DbAssociatedItemList>	GetAssociatedItemLists( ContentType forType );

		DataUpdateShell<DbAssociatedItemList>	GetAssociationForUpdate( long listId );
	}
}
