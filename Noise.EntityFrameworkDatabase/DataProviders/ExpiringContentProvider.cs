using System.Collections.Generic;
using System.Linq;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class ExpiringContentProvider : IExpiringContentProvider {
		private readonly IContextProvider				mContextProvider;

		public ExpiringContentProvider( IContextProvider contextProvider ) {
			mContextProvider = contextProvider;
		}

		public IDataProviderList<ExpiringContent> GetContentList( long forAssociatedItem, ContentType ofType ) {
			IDataProviderList<ExpiringContent>	retValue;

			using( var context = mContextProvider.CreateContext()) {
				var	list = new List<ExpiringContent>();

				list.AddRange( context.Set<DbArtwork>().Where( entity => entity.AssociatedItem == forAssociatedItem && entity.DbContentType == (int)ofType ));
				list.AddRange( context.Set<DbAssociatedItemList>().Where( entity => entity.AssociatedItem == forAssociatedItem && entity.DbContentType == (int)ofType ));
				list.AddRange( context.Set<DbTextInfo>().Where( entity => entity.AssociatedItem == forAssociatedItem && entity.DbContentType == (int)ofType ));

				retValue = new EfProviderList<ExpiringContent>( null, list );
			}

			return( retValue );
		}

		public IDataProviderList<ExpiringContent> GetAlbumContentList( long albumId ) {
			IDataProviderList<ExpiringContent>	retValue;

			using( var context = mContextProvider.CreateContext()) {
				var	list = new List<ExpiringContent>();
		
				list.AddRange( context.Set<DbArtwork>().Where( entity => entity.Album == albumId ));
				list.AddRange( context.Set<DbAssociatedItemList>().Where( entity => entity.Album == albumId ));
				list.AddRange( context.Set<DbTextInfo>().Where( entity => entity.Album == albumId ));

				retValue = new EfProviderList<ExpiringContent>( null, list );
			}

			return( retValue );
		}
	}
}
