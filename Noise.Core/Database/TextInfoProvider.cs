using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class TextInfoProvider : BaseDataProvider<DbTextInfo>, ITextInfoProvider {
		public TextInfoProvider( IDatabaseManager databaseManager ) :
			base( databaseManager ) { }

		private TextInfo TransformTextInfo( DbTextInfo textInfo ) {
			TextInfo	retValue;

			using( var dbShell = CreateDatabase() ) {
				retValue = new TextInfo( textInfo ) { Text = dbShell.Database.BlobStorage.RetrieveText( textInfo.DbId ) };
			}

			return( retValue );
		}
	
		public TextInfo GetArtistTextInfo( long artistId, ContentType ofType ) {
			TextInfo	retValue = null;

			var dbTextInfo = TryGetItem( "SELECT DbTextInfo Where Artist = @artistId AND ContentType = @contentType",
											new Dictionary<string, object> {{ "artistId", artistId }, { "contentType", ofType }}, "Exception - GetArtistArtwork" );

			if( dbTextInfo != null ) {
				retValue = TransformTextInfo( dbTextInfo );
			}

			return( retValue );
		}

		public TextInfo[] GetAlbumTextInfo( long albumId ) {
			TextInfo[]	retValue = null;

			var dbTextInfo = TryGetList( "SELECT DbTextInfo Where Album = @albumId",
											new Dictionary<string, object> {{ "albumId", albumId }}, "Exception - GetAlbumArtwork" );

			if( dbTextInfo != null ) {
				retValue = dbTextInfo.List.Select( TransformTextInfo ).ToArray();
			}

			return( retValue );
		}
	}
}
