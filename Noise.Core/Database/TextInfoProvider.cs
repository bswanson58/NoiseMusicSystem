using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class TextInfoProvider : BaseDataProvider<DbTextInfo>, ITextInfoProvider {
		public TextInfoProvider( IDatabaseManager databaseManager ) :
			base( databaseManager ) { }

		private TextInfo TransformTextInfo( DbTextInfo textInfo ) {
			TextInfo	retValue;

			using( var dbShell = CreateDatabase()) {
				retValue = new TextInfo( textInfo ) { Text = dbShell.Database.BlobStorage.RetrieveText( textInfo.DbId ) };
			}

			return( retValue );
		}

		public void AddTextInfo( DbTextInfo info ) {
			Condition.Requires( info ).IsNotNull();

			using( var dbShell = CreateDatabase()) {
				dbShell.Database.InsertItem( info );
				dbShell.Database.BlobStorage.StoreText( info.DbId, string.Empty );
			}
		}

		public void AddTextInfo( DbTextInfo info, string filePath ) {
			Condition.Requires( info ).IsNotNull();
			Condition.Requires( filePath ).IsNotNullOrEmpty();

			using( var dbShell = CreateDatabase()) {
				dbShell.Database.InsertItem( info );
				dbShell.Database.BlobStorage.Store( info.DbId, filePath );
			}
		}

		public void DeleteTextInfo( DbTextInfo info ) {
			Condition.Requires( info ).IsNotNull();

			using( var dbShell = CreateDatabase()) {
				dbShell.Database.DeleteItem( info );
				dbShell.Database.BlobStorage.Delete( info.DbId );
			}
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

			using( var dbTextInfo = TryGetList( "SELECT DbTextInfo Where Album = @albumId",
											new Dictionary<string, object> {{ "albumId", albumId }}, "Exception - GetAlbumArtwork" )) {
				if( dbTextInfo != null ) {
					retValue = dbTextInfo.List.Select( TransformTextInfo ).ToArray();
				}
			}

			return( retValue );
		}

		public DataUpdateShell<TextInfo> GetTextInfoForUpdate( long textInfoId ) {
			var dbShell = CreateDatabase();
			var dbTextInfo = GetItem( "SELECT DbTextInfo Where DbId = @itemId", new Dictionary<string, object> {{ "itemId", textInfoId }}, dbShell );

			return( new TextInfoUpdateShell( dbShell, dbTextInfo, new TextInfo( dbTextInfo )));
		}
	}

	internal class TextInfoUpdateShell : DataUpdateShell<TextInfo> {
		private readonly DbTextInfo	mTextInfo;

		public TextInfoUpdateShell( IDatabaseShell dbShell, DbTextInfo dbTextInfo, TextInfo info ) :
			base( dbShell, info ) {
			mTextInfo = dbTextInfo;
		}

		public override void Update() {
			if(( mDatabaseShell != null ) &&
			   ( Item != null )) {
				mTextInfo.Copy( Item );
				mDatabaseShell.Database.UpdateItem( mTextInfo );


				if( Item.Text != null ) {
					mDatabaseShell.Database.BlobStorage.StoreText( Item.DbId, Item.Text );
				}
			}
		}
	}
}
