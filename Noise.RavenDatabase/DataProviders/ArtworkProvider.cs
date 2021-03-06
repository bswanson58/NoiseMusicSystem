﻿using System;
using System.IO;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Logging;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	internal class ArtworkProvider : BaseProvider<DbArtwork>, IArtworkProvider {
		public ArtworkProvider( IDbFactory databaseFactory, ILogRaven log ) :
			base( databaseFactory, entity => new object[] { entity.DbId }, log ) {
		}

		private Artwork TransformArtwork( DbArtwork artwork ) {
			return( new Artwork( artwork ) { Image = DbFactory.GetBlobStorage().RetrieveBytes( artwork.DbId ) } );
		}

		public void AddArtwork( DbArtwork artwork ) {
			AddArtwork( artwork, new byte[0] );
		}

		public void AddArtwork( DbArtwork artwork, byte[] pictureData ) {
			Condition.Requires( artwork ).IsNotNull();
			Condition.Requires( pictureData ).IsNotNull();

			try {
				Database.Add( artwork );

				var byteStream = new MemoryStream( pictureData );
				DbFactory.GetBlobStorage().Insert( artwork.DbId, byteStream );
			}
			catch( Exception ex ) {
				Log.LogException( string.Format( "Adding {0}", artwork ), ex );
			}
		}

		public void AddArtwork( DbArtwork artwork, string filePath ) {
			Condition.Requires( artwork ).IsNotNull();
			Condition.Requires( filePath ).IsNotNullOrEmpty();

			try {
				Database.Add( artwork );

				DbFactory.GetBlobStorage().Insert( artwork.DbId, filePath );
			}
			catch( Exception ex ) {
				Log.LogException( string.Format( "Add {0}", artwork ), ex );
			}
		}

		public void DeleteArtwork( DbArtwork artwork ) {
			Condition.Requires( artwork ).IsNotNull();

			try {
				Database.Delete( artwork );

				DbFactory.GetBlobStorage().Delete( artwork.DbId );
			}
			catch( Exception ex ) {
				Log.LogException( string.Format( "Deleting {0}", artwork ), ex );
			}
		}

        public Artwork GetArtwork( long artworkId ) {
            return TransformArtwork( Database.Get( artworkId ));
        }

		public Artwork GetArtistArtwork( long artistId, ContentType ofType ) {
			Artwork	retValue = null;

			var dbArtwork = Database.Get( entity => (( entity.Artist == artistId ) && ( entity.DbContentType == (int)ofType )));

			if( dbArtwork != null ) {
				retValue = TransformArtwork( dbArtwork );
			}

			return ( retValue );
		}

		public Artwork[] GetAlbumArtwork( long albumId, ContentType ofType ) {
            return GetAlbumArtworkInfo( albumId, ofType ).Select( TransformArtwork ).ToArray();
		}

		public Artwork[] GetAlbumArtwork( long albumId ) {
            return GetAlbumArtworkInfo( albumId ).Select( TransformArtwork ).ToArray(); 
		}

	    public DbArtwork[] GetAlbumArtworkInfo( long albumId, ContentType ofType ) {
	        IDataProviderList<DbArtwork>    artworkList;

	        if( ofType == ContentType.AlbumCover ) {
	            artworkList = Database.Find( entity => ((entity.Album == albumId) &&
	                                                    ((entity.DbContentType == (int)ofType) || (entity.IsUserSelection))) );
	        }
	        else {
	            artworkList = Database.Find( entity => ((entity.Album == albumId) && (entity.DbContentType == (int)ofType)) );
	        }

	        return (artworkList.List.ToArray());
	    }

        public DbArtwork[] GetAlbumArtworkInfo( long albumId ) {
            var artworkList = Database.Find( entity => entity.Album == albumId );

            return (artworkList.List.ToArray());
	    }

        public IDataProviderList<DbArtwork> GetArtworkForFolder( long folderId ) {
			return ( Database.Find( entity => entity.FolderLocation == folderId ));
		}

		public IDataUpdateShell<Artwork> GetArtworkForUpdate( long artworkId ) {
			var dbArtwork = Database.Get( artworkId );

			return( new ArtworkUpdateShell( Database, DbFactory.GetBlobStorage(), dbArtwork, TransformArtwork( dbArtwork )));
		}

		public void UpdateArtworkImage( long artworkId, string imageFilePath ) {
			DbFactory.GetBlobStorage().Store( artworkId, imageFilePath );
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
