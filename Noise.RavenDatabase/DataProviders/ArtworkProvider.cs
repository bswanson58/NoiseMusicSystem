using System;
using System.IO;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class ArtworkProvider : IArtworkProvider {
		private readonly IDbFactory				mDbFactory;
		private readonly IRepository<DbArtwork>	mDatabase;

		public ArtworkProvider( IDbFactory databaseFactory ) {
			mDbFactory = databaseFactory;

			mDatabase = new RavenRepository<DbArtwork>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId });
		}

		private Artwork TransformArtwork( DbArtwork artwork ) {
			return( new Artwork( artwork ) { Image = mDbFactory.GetBlobStorage().RetrieveBytes( artwork.DbId ) } );
		}

		public void AddArtwork( DbArtwork artwork ) {
			AddArtwork( artwork, new byte[0] );
		}

		public void AddArtwork( DbArtwork artwork, byte[] pictureData ) {
			Condition.Requires( artwork ).IsNotNull();
			Condition.Requires( pictureData ).IsNotNull();

			try {
				mDatabase.Add( artwork );

				var byteStream = new MemoryStream( pictureData );
				mDbFactory.GetBlobStorage().Insert( artwork.DbId, byteStream );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "AddArtwork", ex );
			}
		}

		public void AddArtwork( DbArtwork artwork, string filePath ) {
			Condition.Requires( artwork ).IsNotNull();
			Condition.Requires( filePath ).IsNotNullOrEmpty();

			try {
				mDatabase.Add( artwork );

				mDbFactory.GetBlobStorage().Insert( artwork.DbId, filePath );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "AddArtwork", ex );
			}
		}

		public void DeleteArtwork( DbArtwork artwork ) {
			Condition.Requires( artwork ).IsNotNull();

			try {
				mDatabase.Delete( artwork );

				mDbFactory.GetBlobStorage().Delete( artwork.DbId );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "DeleteArtwork", ex );
			}
		}

		public Artwork GetArtistArtwork( long artistId, ContentType ofType ) {
			Artwork	retValue = null;

			var dbArtwork = mDatabase.Get( entity => (( entity.Artist == artistId ) && ( entity.DbContentType == (int)ofType )));

			if( dbArtwork != null ) {
				retValue = TransformArtwork( dbArtwork );
			}

			return ( retValue );
		}

		public Artwork[] GetAlbumArtwork( long albumId, ContentType ofType ) {
			IQuerySession<DbArtwork>	artworkList;

			if( ofType == ContentType.AlbumCover ) {
				artworkList = mDatabase.Find( entity => (( entity.Album == albumId ) &&
														(( entity.DbContentType == (int)ofType ) || ( entity.IsUserSelection ))));
			}
			else {
				artworkList = mDatabase.Find( entity => (( entity.Album == albumId ) && ( entity.DbContentType == (int)ofType )));
			}

			return( artworkList.Query().Select( TransformArtwork ).ToArray());
		}

		public Artwork[] GetAlbumArtwork( long albumId ) {
			var artworkList = mDatabase.Find( entity => entity.Album == albumId );

			return( artworkList.Query().Select( TransformArtwork ).ToArray());
		}

		public IDataProviderList<DbArtwork> GetArtworkForFolder( long folderId ) {
			return ( new RavenDataProviderList<DbArtwork>( mDatabase.Find( entity => entity.FolderLocation == folderId )));
		}

		public IDataUpdateShell<Artwork> GetArtworkForUpdate( long artworkId ) {
			var dbArtwork = mDatabase.Get( artworkId );

			return( new ArtworkUpdateShell( mDatabase, mDbFactory.GetBlobStorage(), dbArtwork, TransformArtwork( dbArtwork )));
		}

		public void UpdateArtworkImage( long artworkId, string imageFilePath ) {
			mDbFactory.GetBlobStorage().Store( artworkId, imageFilePath );
		}
	}

	internal class ArtworkUpdateShell : RavenDataUpdateShell<Artwork> {
		private readonly DbArtwork		mArtwork;
		private readonly IBlobStorage	mBlobStorage;

		public ArtworkUpdateShell( IRepository<DbArtwork> database, IBlobStorage blobStorage, DbArtwork dbArtwork, Artwork item ) :
			base( database.Update, item ) {
			mArtwork = dbArtwork;
			mBlobStorage = blobStorage;
		}

		public override void Update() {
			if( Item != null ) {
				mArtwork.Copy( Item );

				base.Update();

				if( Item.Image != null ) {
					var memoryStream = new MemoryStream( Item.Image );

					mBlobStorage.Store( Item.DbId, memoryStream );
				}
			}
		}
	}
}
