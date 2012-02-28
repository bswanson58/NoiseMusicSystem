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
				var artworkList = context.Set<DbArtwork>().Where( entity => entity.AssociatedItem == forAssociatedItem && entity.DbContentType == (int)ofType );
				var associationList = context.Set<DbAssociatedItemList>().Where( entity => entity.AssociatedItem == forAssociatedItem && entity.DbContentType == (int)ofType );
				var discographyList = context.Set<DbDiscographyRelease>().Where( entity => entity.AssociatedItem == forAssociatedItem && entity.DbContentType == (int)ofType );
				var textInfoList = context.Set<DbTextInfo>().Where( entity => entity.AssociatedItem == forAssociatedItem && entity.DbContentType == (int)ofType );

				var	list = new List<ExpiringContent>();
 				list.AddRange( artworkList );
				list.AddRange( associationList );
				list.AddRange( discographyList );
				list.AddRange( textInfoList );

				retValue = new EfProviderList<ExpiringContent>( null, list );
			}

			return( retValue );
		}

		public IDataProviderList<ExpiringContent> GetAlbumContentList( long albumId ) {
			throw new System.NotImplementedException();
		}
	}
}
