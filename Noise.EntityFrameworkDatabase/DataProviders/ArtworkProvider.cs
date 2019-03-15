using System;
using System.IO;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.EntityFrameworkDatabase.Logging;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	internal class ArtworkProvider : BaseProvider<DbArtwork>, IArtworkProvider {
		public ArtworkProvider( IContextProvider contextProvider, ILogDatabase log ) :
			base( contextProvider, log ) { }

		private Artwork TransformArtwork( DbArtwork artwork ) {
			return( new Artwork( artwork ) { Image = BlobStorage.RetrieveBytes( artwork.DbId ) });
		}

		public void AddArtwork( DbArtwork artwork ) {
			AddArtwork( artwork, new byte[0]);
		}

		public void AddArtwork( DbArtwork artwork, byte[] pictureData ) {
			Condition.Requires( artwork ).IsNotNull();
			Condition.Requires( pictureData ).IsNotNull();

			try {
				AddItem( artwork );

				var byteStream = new MemoryStream( pictureData );
				BlobStorage.Insert( artwork.DbId, byteStream );
			}
			catch( Exception ex ) {
				Log.LogException( "AddArtwork", ex );
			}
		}

		public void AddArtwork( DbArtwork artwork, string filePath ) {
			Condition.Requires( artwork ).IsNotNull();
			Condition.Requires( filePath ).IsNotNullOrEmpty();

			try {
				AddItem( artwork );

				BlobStorage.Insert( artwork.DbId, filePath );
			}
			catch( Exception ex ) {
				Log.LogException( "AddArtwork", ex );
			}
		}

		public void DeleteArtwork( DbArtwork artwork ) {
			Condition.Requires( artwork ).IsNotNull();

			try {
				RemoveItem( artwork );

				BlobStorage.Delete( artwork.DbId );
			}
			catch( Exception ex ) {
				Log.LogException( "DeleteArtwork", ex );
			}
		}

        public Artwork GetArtwork( long artworkId ) {
            return TransformArtwork( GetItemByKey( artworkId ));
        }

		public Artwork GetArtistArtwork( long artistId, ContentType ofType ) {
			Artwork	retValue = null;

			using( var context = CreateContext()) {
				var dbArtwork = Set( context ).FirstOrDefault( entity => (( entity.Artist == artistId ) && ( entity.DbContentType == (int)ofType)));

				if( dbArtwork != null ) {
					retValue = TransformArtwork( dbArtwork );
				}
			}

			return( retValue );
		}

		public Artwork[] GetAlbumArtwork( long albumId, ContentType ofType ) {
            return( GetAlbumArtworkInfo( albumId, ofType ).Select( TransformArtwork ).ToArray());
		}

	    public DbArtwork[] GetAlbumArtworkInfo(long albumId, ContentType ofType) {
	        DbArtwork[]   retValue;

	        using( var context = CreateContext() ) {
	            IQueryable<DbArtwork>   artworkList;

	            if( ofType == ContentType.AlbumCover ) {
	                artworkList = Set( context ).Where( entity => ((entity.Album == albumId) &&
	                                                               ((entity.DbContentType == (int)ofType) || (entity.IsUserSelection))) );
	            }
	            else {
	                artworkList = Set( context ).Where( entity => ((entity.Album == albumId) && (entity.DbContentType == (int)ofType)) );
	            }

	            retValue = artworkList.ToArray();
	        }

	        return retValue;
	    }

        public DbArtwork[] GetAlbumArtworkInfo( long albumId ) {
	        DbArtwork[]   retValue;

	        using( var context = CreateContext() ) {
	            var artworkList = Set( context ).Where( entity => entity.Album == albumId );

	            retValue = artworkList.ToArray();
	        }

	        return (retValue);
	    }

	    public Artwork[] GetAlbumArtwork( long albumId ) {
            return( GetAlbumArtworkInfo( albumId ).Select( TransformArtwork ).ToArray());
	    }

        public IDataProviderList<DbArtwork> GetArtworkForFolder( long folderId ) {
			var context = CreateContext();

			return( new EfProviderList<DbArtwork>( context, Set( context ).Where( entity => entity.FolderLocation == folderId )));
		}

		public IDataUpdateShell<Artwork> GetArtworkForUpdate( long artworkId ) {
			var context = CreateContext();
			var dbArtwork = GetItemByKey( context, artworkId );

			return( new ArtworkUpdateShell( context, BlobStorage, dbArtwork, TransformArtwork( dbArtwork )));
		}

		public void UpdateArtworkImage( long artworkId, string imageFilePath ) {
			BlobStorage.Store( artworkId, imageFilePath );
		}
	}

	internal class ArtworkUpdateShell : EfUpdateShell<Artwork> {
		private readonly DbArtwork		mArtwork;
		private readonly IBlobStorage	mBlobStorage;

		public ArtworkUpdateShell( IDbContext context, IBlobStorage blobStorage, DbArtwork dbArtwork, Artwork item ) :
			base( context, item ) {
			mArtwork = dbArtwork;
			mBlobStorage = blobStorage;
		}

		public override void Update() {
			if(( mContext != null ) &&
			   ( Item != null )) {
				mArtwork.Copy( Item );
				mContext.SaveChanges();

				if( Item.Image != null ) {
					var memoryStream = new MemoryStream( Item.Image );

					mBlobStorage.Store( Item.DbId, memoryStream );
				}
			}
		}
	}
}
