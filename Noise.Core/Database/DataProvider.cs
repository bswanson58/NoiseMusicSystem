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

			if( dbObject is DbBase ) {
				retValue = ( dbObject as DbBase ).DbId;
			}
			else {
				mLog.LogMessage( "DbId requested for non DbBase object {0}", dbObject.GetType());
			}
			return( retValue );
		}

		public void UpdateItem( object item ) {
			Condition.Requires( item ).IsNotNull();

			if(( item is DbArtist ) ||
			   ( item is DbAlbum ) ||
			   ( item is DbTrack ) ||
			   ( item is DbInternetStream )) {
				var database = mDatabaseManager.ReserveDatabase();

				try {
					database.Store( item );
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - UpdateItem:", ex );
				}
				finally {
					mDatabaseManager.FreeDatabase( database );
				}
			}
		}

		public void DeleteItem( object dbItem ) {
			Condition.Requires( dbItem ).IsNotNull();

			if( dbItem is DbInternetStream ) {
				var database = mDatabaseManager.ReserveDatabase();

				try {
					database.Delete( dbItem );
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - UpdateItem:", ex );
				}
				finally {
					mDatabaseManager.FreeDatabase( database );
				}
			}
		}

		public DataProviderList<DbArtist> GetArtistList() {
			DataProviderList<DbArtist>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();

			try {
				retValue = new DataProviderList<DbArtist>( database.DatabaseId, FreeDatabase, from DbArtist artist in database.Database select artist );
			}
			catch( Exception ex ) {
				mLog.LogException( ex );

				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public DataProviderList<DbArtist> GetArtistList( IDatabaseFilter filter ) {
			Condition.Requires( filter ).IsNotNull();

			DataProviderList<DbArtist>	retValue = null;

			if( filter.IsEnabled ) {
				var database = mDatabaseManager.ReserveDatabase();

				try {
					retValue = new DataProviderList<DbArtist>( database.DatabaseId, FreeDatabase,
																from DbArtist artist in database.Database where filter.ArtistMatch( artist ) select artist );
				}
				catch( Exception ex ) {
					mLog.LogException( ex );

					mDatabaseManager.FreeDatabase( database );
				}
			}
			else {
				retValue = GetArtistList();
			}

			return( retValue );
		}

		public DbArtist GetArtistForAlbum( DbAlbum album ) {
			Condition.Requires( album ).IsNotNull();

			DbArtist	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();

			try {
				retValue = ( from DbArtist artist in database.Database where artist.DbId == album.Artist select artist ).FirstOrDefault();
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetArtistForAlbum:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public DataProviderList<DbAlbum> GetAlbumList( DbArtist forArtist ) {
			Condition.Requires( forArtist ).IsNotNull();

			DataProviderList<DbAlbum>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();

			try {
				var artistId = GetObjectIdentifier( forArtist );

				retValue = new DataProviderList<DbAlbum>( database.DatabaseId, FreeDatabase,
															from DbAlbum album in database.Database where album.Artist == artistId select album );
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetAlbumList:", ex );

				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public DbAlbum GetAlbumForTrack( DbTrack track ) {
			Condition.Requires( track ).IsNotNull();

			DbAlbum	retValue = null;
			var		database = mDatabaseManager.ReserveDatabase();

			try {
				retValue = ( from DbAlbum album in database.Database where album.DbId == track.Album select album ).FirstOrDefault();
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetAlbumForTrack:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public DataProviderList<DbTrack> GetTrackList( DbAlbum forAlbum ) {
			Condition.Requires( forAlbum ).IsNotNull();

			DataProviderList<DbTrack>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				var albumId = GetObjectIdentifier( forAlbum );

				retValue = new DataProviderList<DbTrack>( database.DatabaseId, FreeDatabase,
															from DbTrack track in database.Database where track.Album == albumId select track );
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetTrackList(forAlbum):", ex );

				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public List<DbTrack> GetTrackList( DbArtist forArtist ) {
			Condition.Requires( forArtist ).IsNotNull();

			var	retValue = new List<DbTrack>();
			var database =mDatabaseManager.ReserveDatabase();

			try {
				var artistId = GetObjectIdentifier( forArtist );
				var albumList = from DbAlbum album in database.Database where album.Artist == artistId select album;

				foreach( DbAlbum album in albumList ) {
					var albumId = GetObjectIdentifier( album );
					var trackList = from DbTrack track in database.Database where track.Album == albumId select track;

					retValue.AddRange( trackList );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetTrackList(forArtist):", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public StorageFile GetPhysicalFile( DbTrack forTrack ) {
			Condition.Requires( forTrack ).IsNotNull();

			StorageFile	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				var trackId = GetObjectIdentifier( forTrack );

				Condition.Requires( trackId ).IsNotLessOrEqual( 0 );

				retValue = ( from StorageFile file in database.Database where file.MetaDataPointer == trackId select file ).FirstOrDefault();
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetPhysicalFile:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
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
			var database = mDatabaseManager.ReserveDatabase();
			try {
				var artistId = GetObjectIdentifier( forArtist );
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
				mDatabaseManager.FreeDatabase( database );
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

			var database = mDatabaseManager.ReserveDatabase();
			try {
				var albumId = GetObjectIdentifier( forAlbum );
				var albumTrack = ( from DbTrack track in database.Database where track.Album == albumId select track ).FirstOrDefault();

				if( albumTrack != null ) {
					var trackId = GetObjectIdentifier( albumTrack );
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
				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public DataProviderList<DbInternetStream> GetStreamList() {
			DataProviderList<DbInternetStream>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
			retValue = new DataProviderList<DbInternetStream>( database.DatabaseId, FreeDatabase,
																from DbInternetStream stream in database.Database select stream );
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetStreamList:", ex );

				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}
	}
}
