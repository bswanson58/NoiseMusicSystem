using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
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

		public void AddArtwork( DbArtwork artwork, byte[] pictureData ) {
			Condition.Requires( artwork ).IsNotNull();
			Condition.Requires( pictureData ).IsNotEmpty();

			try {
				using( var dbShell = CreateDatabase()) {
					dbShell.InsertItem( artwork );

					var byteStream = new MemoryStream( pictureData );
					dbShell.Database.BlobStorage.Insert( artwork.DbId, byteStream );
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "AddArtwork", ex );
			}
		}

		public void AddArtwork( DbArtwork artwork, string filePath ) {
			Condition.Requires( artwork ).IsNotNull();
			Condition.Requires( filePath ).IsNotNullOrEmpty();

			try {
				using( var dbShell = CreateDatabase()) {
					dbShell.InsertItem( artwork );

					dbShell.Database.BlobStorage.Insert( artwork.DbId, filePath );
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "AddArtwork", ex );
			}
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
			var			query = "SELECT DbArtwork Where Album = @albumId AND ContentType = @contentType";

			if( ofType == ContentType.AlbumCover ) {
				query = "SELECT DbArtwork Where Album = @albumId AND ( ContentType = @contentType OR IsUserSelection )";
			}

			var dbArtworkList = TryGetList( query, new Dictionary<string, object> {{ "albumId", albumId }, { "contentType", ofType }}, "Exception - GetAlbumArtwork" );

			if( dbArtworkList != null ) {
				retValue = dbArtworkList.List.Select( TransformArtwork ).ToArray();
			}

			return( retValue );
		}

		public DataProviderList<DbArtwork> GetArtworkForFolder( long folderId ) {
			return( TryGetList( "SELECT DbArtwork Where FolderLocation = @folderId", new Dictionary<string, object> {{ "folderId", folderId }}, "GetArtworkForFolder" ));
		}

		public DataUpdateShell<DbArtwork> GetArtworkForUpdate( long artworkId ) {
			return( GetUpdateShell( "SELECT DbArtwork Where DbId = @artworkId", new Dictionary<string, object> {{ "artworkId", artworkId }} ));
		}
	}
}
