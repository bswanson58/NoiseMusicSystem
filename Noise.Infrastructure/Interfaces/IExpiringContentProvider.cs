using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IExpiringContentProvider {
		IDataProviderList<ExpiringContent>	GetContentList( long forAssociatedItem, ContentType ofType );
		IDataProviderList<ExpiringContent>	GetAlbumContentList( long albumId );
	}
}
