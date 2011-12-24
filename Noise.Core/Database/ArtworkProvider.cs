using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class ArtworkProvider : BaseDataProvider<DbArtwork>, IArtworkProvider {
		public ArtworkProvider( IDatabaseManager databaseManager )
			: base( databaseManager ) {
		}

		private Artwork TransformArtwork( DbArtwork artwork ) {
			Artwork	retValue;

			using( var dbShell = CreateDatabase() ) {
				retValue = new Artwork( artwork ) { Image = dbShell.Database.BlobStorage.RetrieveBytes( artwork.DbId ) };
			}

			return( retValue );
		}

		public Artwork GetArtistArtwork( long artistId, ContentType ofType ) {
			Artwork	retValue = null;

			var dbArtwork = TryGetItem( "SELECT DbArtwork Where Artist = @artistId AND ContentType = @contentType",
				new Dictionary<string, object> {{ "artistId", artistId }, { "contentType", ofType }}, "Exception - GetArtistArtwork" );

			if( dbArtwork != null ) {
				retValue = TransformArtwork( dbArtwork );
			}

			return( retValue );
		}

		public Artwork[] GetAlbumArtwork( long albumId, ContentType ofType ) {
			Artwork[]	retValue = null;

			var dbArtworkList = TryGetList( "SELECT DbArtwork Where Album = @albumId AND ContentType = @contentType",
											new Dictionary<string, object> {{ "albumId", albumId }, { "contentType", ofType }}, "Exception - GetAlbumArtwork" );

			if( dbArtworkList != null ) {
				retValue = dbArtworkList.List.Select( TransformArtwork ).ToArray();
			}

			return( retValue );
		}

		public DataUpdateShell<DbArtwork> GetArtworkForUpdate( long artworkId ) {
			throw new NotImplementedException();
		}
	}
}
