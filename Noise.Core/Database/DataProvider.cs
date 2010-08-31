using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Microsoft.Practices.Unity;
using Noise.Core.DataBuilders;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	public class DataProvider : IDataProvider {
		private readonly IUnityContainer	mContainer;
		private readonly IDatabaseManager	mDatabaseManager;
		private readonly IContentManager	mContentManager;
		private readonly ILog				mLog;

		public DataProvider( IUnityContainer container ) {
			mContainer = container;
			mDatabaseManager = mContainer.Resolve<IDatabaseManager>();
			mContentManager = mContainer.Resolve<IContentManager>();
			mLog = mContainer.Resolve<ILog>();
		}

		private void FreeDatabase( string databaseId ) {
			mDatabaseManager.FreeDatabase( databaseId );
		}

		public long GetObjectIdentifier( object dbObject ) {
			Condition.Requires( dbObject ).IsNotNull();

			long	retValue = 0L;
			var		database = mDatabaseManager.ReserveDatabase( "GetObjectIdentifier" );

			try {
				retValue = database.Database.GetUid( dbObject );

				Condition.Requires( retValue ).IsNotEqualTo( -1 );
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetObjectIdentifier:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database.DatabaseId );
			}

			return( retValue );
		}

		public void UpdateItem( object item ) {
			Condition.Requires( item ).IsNotNull();

			if(( item is DbArtist ) ||
			   ( item is DbAlbum ) ||
			   ( item is DbTrack ) ||
			   ( item is DbInternetStream )) {
				var database = mDatabaseManager.ReserveDatabase( "UpdateItem" );

				try {
					database.Database.Store( item );
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - UpdateItem:", ex );
				}
				finally {
					mDatabaseManager.FreeDatabase( database.DatabaseId );
				}
			}
		}

		public void DeleteItem( object dbItem ) {
			Condition.Requires( dbItem ).IsNotNull();

			if( dbItem is DbInternetStream ) {
				var database = mDatabaseManager.ReserveDatabase( "UpdateItem" );

				try {
					database.Database.Delete( dbItem );
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - UpdateItem:", ex );
				}
				finally {
					mDatabaseManager.FreeDatabase( database.DatabaseId );
				}
			}
		}

		public DataProviderList<DbArtist> GetArtistList() {
			DataProviderList<DbArtist>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase( "GetArtistList" );

			try {
				retValue = new DataProviderList<DbArtist>( database.DatabaseId, FreeDatabase, from DbArtist artist in database.Database select artist );
			}
			catch( Exception ex ) {
				mLog.LogException( ex );

				mDatabaseManager.FreeDatabase( database.DatabaseId );
			}

			return( retValue );
		}

		public DbArtist GetArtistForAlbum( DbAlbum album ) {
			Condition.Requires( album ).IsNotNull();

			DbArtist	retValue = null;

			var database = mDatabaseManager.ReserveDatabase( "GetArtistForAlbum" );

			try {
				var parms = database.Database.CreateParameters();

				parms["artistId"] = album.Artist;
				retValue = database.Database.ExecuteScalar( "SELECT DbArtist WHERE $ID = @artistId", parms ) as DbArtist;
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetArtistForAlbum:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database.DatabaseId );
			}

			return( retValue );
		}

		public DataProviderList<DbAlbum> GetAlbumList( DbArtist forArtist ) {
			Condition.Requires( forArtist ).IsNotNull();

			DataProviderList<DbAlbum>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase( "GetAlbumList" );

			try {
				var artistId = database.Database.GetUid( forArtist );

				retValue = new DataProviderList<DbAlbum>( database.DatabaseId, FreeDatabase,
															from DbAlbum album in database.Database where album.Artist == artistId select album );
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetAlbumList:", ex );

				mDatabaseManager.FreeDatabase( database.DatabaseId );
			}

			return( retValue );
		}

		public DbAlbum GetAlbumForTrack( DbTrack track ) {
			Condition.Requires( track ).IsNotNull();

			DbAlbum	retValue = null;
			var		database = mDatabaseManager.ReserveDatabase( "GetAlbumList" );

			try {
				var parms = database.Database.CreateParameters();

				parms["albumId"] = track.Album;

				retValue = database.Database.ExecuteScalar( "SELECT DbAlbum WHERE $ID = @albumId", parms ) as DbAlbum;
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetAlbumForTrack:", ex );
			}

			return( retValue );
		}

		public DataProviderList<DbTrack> GetTrackList( DbAlbum forAlbum ) {
			Condition.Requires( forAlbum ).IsNotNull();

			DataProviderList<DbTrack>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase( "GetTrackList" );
			try {
				var albumId = database.Database.GetUid( forAlbum );

				retValue = new DataProviderList<DbTrack>( database.DatabaseId, FreeDatabase,
															from DbTrack track in database.Database where track.Album == albumId select track );
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetTrackList(forAlbum):", ex );

				mDatabaseManager.FreeDatabase( database.DatabaseId );
			}

			return( retValue );
		}

		public List<DbTrack> GetTrackList( DbArtist forArtist ) {
			Condition.Requires( forArtist ).IsNotNull();

			var	retValue = new List<DbTrack>();
			var database =mDatabaseManager.ReserveDatabase( "GetTrackList" );

			try {
				var artistId = database.Database.GetUid( forArtist );
				var albumList = from DbAlbum album in database.Database where album.Artist == artistId select album;

				foreach( DbAlbum album in albumList ) {
					var albumId = database.Database.GetUid( album );
					var trackList = from DbTrack track in database.Database where track.Album == albumId select track;

					retValue.AddRange( trackList );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetTrackList(forArtist):", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database.DatabaseId );
			}

			return( retValue );
		}

		public StorageFile GetPhysicalFile( DbTrack forTrack ) {
			Condition.Requires( forTrack ).IsNotNull();

			StorageFile	retValue = null;

			var database = mDatabaseManager.ReserveDatabase( "GetPhysicalFile" );
			try {
				var trackId = database.Database.GetUid( forTrack );

				Condition.Requires( trackId ).IsNotLessOrEqual( 0 );

				retValue = ( from StorageFile file in database.Database where file.MetaDataPointer == trackId select file ).FirstOrDefault();
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetPhysicalFile:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database.DatabaseId );
			}

			return( retValue );
		}

		public object GetMetaData( StorageFile forFile ) {
			Condition.Requires( forFile ).IsNotNull();

			object	retValue = null;
			var		database = mDatabaseManager.ReserveDatabase( "GetMetaData" );

			try {
				var parm = database.Database.CreateParameters();

				parm["id"] = forFile.MetaDataPointer;

				retValue = database.Database.ExecuteScalar( "SELECT data WHERE $ID = @id", parm );
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetMetaData:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database.DatabaseId );
			}

			return( retValue );
		}

		public void UpdateArtistInfo( DbArtist forArtist ) {
			Condition.Requires( forArtist ).IsNotNull();

			mContentManager.RequestContent( forArtist );
		}

		public ArtistSupportInfo GetArtistSupportInfo( DbArtist forArtist ) {
			Condition.Requires( forArtist ).IsNotNull();

			ArtistSupportInfo	retValue = null;
			var database = mDatabaseManager.ReserveDatabase( "GetArtistSupportInfo" );
			try {
				var artistId = database.Database.GetUid( forArtist );
				var parms = database.Database.CreateParameters();

				parms["artistId"] = artistId;
				parms["artistImage"] = ContentType.ArtistPrimaryImage;

				retValue = new ArtistSupportInfo(( from DbTextInfo bio in database.Database where bio.AssociatedItem == artistId && bio.ContentType == ContentType.Biography select bio ).FirstOrDefault(),
												   database.Database.ExecuteScalar( "SELECT DbArtwork Where AssociatedItem = @artistId AND ContentType = @artistImage", parms ) as DbArtwork,
												 ( from DbAssociatedItems item in database.Database where item.AssociatedItem == artistId && item.ContentType == ContentType.SimilarArtists select item ).FirstOrDefault(),
												 ( from DbAssociatedItems item in database.Database where item.AssociatedItem == artistId && item.ContentType == ContentType.TopAlbums select item ).FirstOrDefault(),
												 ( from DbAssociatedItems item in database.Database where item.AssociatedItem == artistId && item.ContentType == ContentType.BandMembers select item ).FirstOrDefault());
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetArtistSupportInfo:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database.DatabaseId );
			}

			return( retValue );
		}

		public void UpdateAlbumInfo( DbAlbum forAlbum ) {
			Condition.Requires( forAlbum ).IsNotNull();

			mContentManager.RequestContent( forAlbum );
		}

		public AlbumSupportInfo GetAlbumSupportInfo( DbAlbum forAlbum ) {
			Condition.Requires( forAlbum ).IsNotNull();

			AlbumSupportInfo	retValue = null;

			var database = mDatabaseManager.ReserveDatabase( "GetAlbumSupportInfo" );
			try {
				var albumId = database.Database.GetUid( forAlbum );
				var albumTrack = ( from DbTrack track in database.Database where track.Album == albumId select track ).FirstOrDefault();

				if( albumTrack != null ) {
					var trackId = database.Database.GetUid( albumTrack );
					var	fileTrack = ( from StorageFile file in database.Database where file.MetaDataPointer == trackId select file ).FirstOrDefault();

					if( fileTrack != null ) {
						var parms = database.Database.CreateParameters();

						parms["folderId"] = fileTrack.ParentFolder;
						parms["coverType"] = ContentType.AlbumCover;
						parms["otherType"] = ContentType.AlbumArtwork;

						retValue = new AlbumSupportInfo( database.Database.ExecuteQuery( "SELECT DbArtwork WHERE FolderLocation = @folderId AND ContentType = @coverType", parms ).OfType<DbArtwork>().ToArray(),
														 database.Database.ExecuteQuery( "SELECT DbArtwork WHERE FolderLocation = @folderId AND ContentType = @otherType", parms ).OfType<DbArtwork>().ToArray(),
														( from DbTextInfo info in database.Database where info.FolderLocation == fileTrack.ParentFolder select info ).ToArray());
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetAlbumSupportInfo:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database.DatabaseId );
			}

			return( retValue );
		}

		public DataProviderList<DbInternetStream> GetStreamList() {
			DataProviderList<DbInternetStream>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase( "GetStreamList" );
			try {
			retValue = new DataProviderList<DbInternetStream>( database.DatabaseId, FreeDatabase,
																from DbInternetStream stream in database.Database select stream );
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetStreamList:", ex );

				mDatabaseManager.FreeDatabase( database.DatabaseId );
			}

			return( retValue );
		}
	}
}
