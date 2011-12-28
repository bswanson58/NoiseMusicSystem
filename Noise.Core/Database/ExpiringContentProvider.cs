using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class ExpiringContentProvider : BaseDataProvider<ExpiringContent>, IExpiringContentProvider {
		public ExpiringContentProvider( IDatabaseManager databaseManager ) :
			base( databaseManager ) { }

		public DataProviderList<ExpiringContent> GetContentList( long forAssociatedItem, ContentType ofType ) {
			return( TryGetList( "SELECT ExpiringContent WHERE AssociatedItem = @associatedItem AND ContentType = @contentType", 
								new Dictionary<string, object> {{ "associatedItem", forAssociatedItem }, { "contentType", ofType }}, "GetContentList" ));
		}

		public DataProviderList<ExpiringContent> GetAlbumContentList( long albumId ) {
			return( TryGetList( "SELECT ExpiringContent WHERE Album = @albumId", new Dictionary<string, object> {{ "albumId", albumId }}, "GetAlbumContentList" ));
		}
	}
}
