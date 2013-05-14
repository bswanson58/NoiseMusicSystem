using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class TextInfoProvider : ITextInfoProvider {
		private readonly IDbFactory					mDbFactory;
		private readonly IRepository<DbTextInfo>	mDatabase;

		public TextInfoProvider( IDbFactory databaseFactory ) {
			mDbFactory = databaseFactory;

			mDatabase = new RavenRepositoryT<DbTextInfo>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId });
		}

		public void AddTextInfo( DbTextInfo info, string filePath ) {
			Condition.Requires( info ).IsNotNull();
			Condition.Requires( filePath ).IsNotNullOrEmpty();

			mDatabase.Add( info );
			mDbFactory.GetBlobStorage().StoreText( info.DbId, string.Empty );
		}

		public void AddTextInfo( DbTextInfo info ) {
			Condition.Requires( info ).IsNotNull();

			mDatabase.Add( info );
			mDbFactory.GetBlobStorage().StoreText( info.DbId, string.Empty );
		}

		public void DeleteTextInfo( DbTextInfo textInfo ) {
			Condition.Requires( textInfo ).IsNotNull();

			mDatabase.Delete( textInfo );
			mDbFactory.GetBlobStorage().Delete( textInfo.DbId );
		}

		public TextInfo GetArtistTextInfo( long artistId, ContentType ofType ) {
			TextInfo	retValue = null;

			var	dbTextInfo = mDatabase.Get( entity => (( entity.Artist == artistId ) && ( entity.ContentType == ofType )));

			if( dbTextInfo != null ) {
				retValue = TransformTextInfo( dbTextInfo );
			}

			return ( retValue );
		}

		public TextInfo[] GetAlbumTextInfo( long albumId ) {
			var	dbTextInfoList = mDatabase.Find( entity => entity.Album == albumId );

			return( dbTextInfoList.Query().Select( TransformTextInfo ).ToArray());
		}

		public IDataUpdateShell<TextInfo> GetTextInfoForUpdate( long textInfoId ) {
			var dbTextInfo = mDatabase.Get( textInfoId );

			return ( new TextUpdateShell( mDatabase, mDbFactory.GetBlobStorage(), dbTextInfo, TransformTextInfo( dbTextInfo )));
		}

		public void UpdateTextInfo( long infoId, string infoFilePath ) {
			mDbFactory.GetBlobStorage().Store( infoId, infoFilePath );
		}

		private TextInfo TransformTextInfo( DbTextInfo textInfo ) {
			return ( new TextInfo( textInfo ) { Text = mDbFactory.GetBlobStorage().RetrieveText( textInfo.DbId ) } );
		}
	}

	internal class TextUpdateShell : RavenDataUpdateShell<TextInfo> {
		private readonly DbTextInfo					mTextInfo;
		private readonly IBlobStorage				mBlobStorage;

		public TextUpdateShell( IRepository<DbTextInfo> database, IBlobStorage blobStorage, DbTextInfo textInfo, TextInfo item ) :
			base( database.Update, item ) {
			mTextInfo = textInfo;
			mBlobStorage = blobStorage;
		}

		public override void Update() {
			if( Item != null ) {
				mTextInfo.Copy( Item );

				base.Update();

				if( Item.Text != null ) {
					mBlobStorage.StoreText( Item.DbId, Item.Text );
				}
			}
		}
	}
}
