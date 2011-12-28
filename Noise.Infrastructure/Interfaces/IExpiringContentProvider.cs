using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IExpiringContentProvider {
		DataProviderList<ExpiringContent>	GetContentList( long forAssociatedItem, ContentType ofType );
		DataProviderList<ExpiringContent>	GetAlbumContentList( long albumId );
	}
}
