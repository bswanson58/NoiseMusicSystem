using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IAssociatedItemListProvider {
		DbAssociatedItemList	GetAssociatedItems( long artistId, ContentType ofType );
	}
}
