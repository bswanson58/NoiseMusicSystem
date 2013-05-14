using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class TextInfoProvider : BaseProvider<DbTextInfo>, ITextInfoProvider {
		public TextInfoProvider( IDbFactory databaseFactory ) :
			base( databaseFactory, entity => new object[] { entity.DbId }) {
		}

		public void AddTextInfo( DbTextInfo info, string filePath ) {
			Condition.Requires( info ).IsNotNull();
			Condition.Requires( filePath ).IsNotNullOrEmpty();

			Database.Add( info );
			DbFactory.GetBlobStorage().StoreText( info.DbId, string.Empty );
		}

		public void AddTextInfo( DbTextInfo info ) {
			Condition.Requires( info ).IsNotNull();

			Database.Add( info );
			DbFactory.GetBlobStorage().StoreText( info.DbId, string.Empty );
		}

		public void DeleteTextInfo( DbTextInfo textInfo ) {
			Condition.Requires( textInfo ).IsNotNull();

			Database.Delete( textInfo );
			DbFactory.GetBlobStorage().Delete( textInfo.DbId );
		}

		public TextInfo GetArtistTextInfo( long artistId, ContentType ofType ) {
			TextInfo	retValue = null;

			var	dbTextInfo = Database.Get( entity => (( entity.Artist == artistId ) && ( entity.ContentType == ofType )));

			if( dbTextInfo != null ) {
				retValue = TransformTextInfo( dbTextInfo );
			}

			return ( retValue );
		}

		public TextInfo[] GetAlbumTextInfo( long albumId ) {
			var	dbTextInfoList = Database.Find( entity => entity.Album == albumId );

			return( dbTextInfoList.Query().Select( TransformTextInfo ).ToArray());
		}

		public IDataUpdateShell<TextInfo> GetTextInfoForUpdate( long textInfoId ) {
			var dbTextInfo = Database.Get( textInfoId );

			return ( new TextUpdateShell( Database, DbFactory.GetBlobStorage(), dbTextInfo, TransformTextInfo( dbTextInfo )));
		}

		public void UpdateTextInfo( long infoId, string infoFilePath ) {
			DbFactory.GetBlobStorage().Store( infoId, infoFilePath );
		}

		private TextInfo TransformTextInfo( DbTextInfo textInfo ) {
			return ( new TextInfo( textInfo ) { Text = DbFactory.GetBlobStorage().RetrieveText( textInfo.DbId ) } );
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
