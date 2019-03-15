using System.Linq;
using CuttingEdge.Conditions;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.EntityFrameworkDatabase.Logging;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	internal class TextInfoProvider : BaseProvider<DbTextInfo>, ITextInfoProvider {
		public TextInfoProvider( IContextProvider contextProvider, ILogDatabase log ) :
			base( contextProvider, log ) { }

		private TextInfo TransformTextInfo( DbTextInfo textInfo ) {
			return( new TextInfo( textInfo ) { Text = BlobStorage.RetrieveText( textInfo.DbId ) });
		}

		public void AddTextInfo( DbTextInfo info, string filePath ) {
			Condition.Requires( info ).IsNotNull();
			Condition.Requires( filePath ).IsNotNullOrEmpty();

			AddItem( info );
			BlobStorage.Insert( info.DbId, filePath );
		}

		public void AddTextInfo( DbTextInfo info ) {
			Condition.Requires( info ).IsNotNull();

			AddItem( info );
			BlobStorage.StoreText( info.DbId, string.Empty );
		}

		public void DeleteTextInfo( DbTextInfo textInfo ) {
			Condition.Requires( textInfo ).IsNotNull();

			RemoveItem( textInfo );
			BlobStorage.Delete( textInfo.DbId );
		}

		public TextInfo GetArtistTextInfo( long artistId, ContentType ofType ) {
			TextInfo	retValue = null;

			using( var context = CreateContext()) {
				var	dbTextInfo = Set( context ).FirstOrDefault( entity => (( entity.Artist == artistId ) && ( entity.DbContentType == (int)ofType )));
			
				if( dbTextInfo != null ) {
					retValue = TransformTextInfo( dbTextInfo );
				}
			}

			return( retValue );
		}

		public TextInfo[] GetAlbumTextInfo( long albumId ) {
			TextInfo[]	retValue;

			using( var context = CreateContext()) {
				var	dbTextInfoList = Set( context ).Where( entity => entity.Album == albumId );
			
				retValue = dbTextInfoList.Select( TransformTextInfo ).ToArray();
			}

			return( retValue );
		}

		public IDataUpdateShell<TextInfo> GetTextInfoForUpdate( long textInfoId ) {
			var context = CreateContext();
			var dbTextInfo = GetItemByKey( context, textInfoId );

			return( new TextUpdateShell( context, BlobStorage, dbTextInfo, TransformTextInfo( dbTextInfo )));
		}

		public void UpdateTextInfo( long infoId, string infoFilePath ) {
			BlobStorage.Store( infoId, infoFilePath );
		}
	}

	internal class TextUpdateShell : EfUpdateShell<TextInfo> {
		private readonly DbTextInfo		mTextInfo;
		private readonly IBlobStorage	mBlobStorage;

		public TextUpdateShell( IDbContext context, IBlobStorage blobStorage, DbTextInfo textInfo, TextInfo item ) :
			base( context, item ) {
			mTextInfo = textInfo;
			mBlobStorage = blobStorage;
		}

		public override void Update() {
			if(( mContext != null ) &&
			   ( Item != null )) {
				mTextInfo.Copy( Item );
				mContext.SaveChanges();

				if( Item.Text != null ) {
					mBlobStorage.StoreText( Item.DbId, Item.Text );
				}
			}
		}
	}
}
