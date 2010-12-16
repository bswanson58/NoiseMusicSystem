using System;
using System.Linq;
using CuttingEdge.Conditions;
using Microsoft.Practices.Unity;
using Noise.Core.DataBuilders;
using Noise.Core.FileStore;
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

		public DbBase GetItem( long itemId ) {
			DbBase	retValue = null;
			var database = mDatabaseManager.ReserveDatabase();

			try {
				var parms = database.Database.CreateParameters();

				parms["itemId"] = itemId;

				retValue = database.Database.ExecuteScalar( "SELECT DbBase Where DbId = @itemId", parms ) as DbBase;
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetItem:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public void InsertItem( object item ) {
			Condition.Requires( item ).IsNotNull();

			if(( item is DbArtist ) ||
			   ( item is DbAlbum ) ||
			   ( item is DbTrack ) ||
			   ( item is DbGenre ) ||
			   ( item is DbInternetStream )) {
				var database = mDatabaseManager.ReserveDatabase();

				try {
					database.Insert( item );
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - InsertItem:", ex );
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

		public DbArtist GetArtist( long dbid ) {
			var			database = mDatabaseManager.ReserveDatabase();
			DbArtist	retValue = null;

			try {
				var parms = database.Database.CreateParameters();

				parms["itemId"] = dbid;

				retValue = database.Database.ExecuteScalar( "SELECT DbArtist Where DbId = @itemId", parms ) as DbArtist;
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetArtist:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public DataProviderList<DbArtist> GetArtistList() {
			DataProviderList<DbArtist>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();

			try {
				retValue = new DataProviderList<DbArtist>( database.DatabaseId, FreeDatabase,
														   database.Database.ExecuteQuery( "SELECT DbArtist" ).OfType<DbArtist>());
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
			var parms = database.Database.CreateParameters();
			parms["artistId"] = album.Artist;

			try {
				retValue = database.Database.ExecuteScalar( "SELECT DbArtist Where DbId = @artistId", parms ) as DbArtist;
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetArtistForAlbum:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public DataProviderList<DbArtist> GetFavoriteArtists() {
			DataProviderList<DbArtist>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				retValue = new DataProviderList<DbArtist>( database.DatabaseId, FreeDatabase,
														   database.Database.ExecuteQuery( "SELECT DbArtist WHERE IsFavorite = true" ).OfType<DbArtist>());
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetFavoriteArtists: ", ex );

				mDatabaseManager.FreeDatabase( database );
			}
			return( retValue );
		}

		public DbAlbum GetAlbum( long dbid ) {
			var			database = mDatabaseManager.ReserveDatabase();
			DbAlbum		retValue = null;

			try {
				var parms = database.Database.CreateParameters();

				parms["itemId"] = dbid;

				retValue = database.Database.ExecuteScalar( "SELECT DbAlbum Where DbId = @itemId", parms ) as DbAlbum;
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetAlbum:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public DataProviderList<DbAlbum> GetAlbumList( DbArtist forArtist ) {
			Condition.Requires( forArtist ).IsNotNull();

			return( GetAlbumList( forArtist.DbId ));
		}

		public DataProviderList<DbAlbum> GetAlbumList( long artistId ) {
			DataProviderList<DbAlbum>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			var parms = database.Database.CreateParameters();

			parms["artistId"] = artistId;

			try {
				retValue = new DataProviderList<DbAlbum>( database.DatabaseId, FreeDatabase,
														  database.Database.ExecuteQuery( "SELECT DbAlbum WHERE Artist = @artistId", parms ).OfType<DbAlbum>());
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
			var	database = mDatabaseManager.ReserveDatabase();
			var parms = database.Database.CreateParameters();
			parms["albumId"] = track.Album;

			try {
				retValue = database.Database.ExecuteScalar( "SELECT DbAlbum Where DbId = @albumId", parms ) as DbAlbum;
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetAlbumForTrack:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public DataProviderList<DbAlbum> GetFavoriteAlbums() {
			DataProviderList<DbAlbum>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				retValue = new DataProviderList<DbAlbum>( database.DatabaseId, FreeDatabase,
														  database.Database.ExecuteQuery( "SELECT DbAlbum WHERE IsFavorite = true" ).OfType<DbAlbum>());
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetFavoriteAlbums: ", ex );

				mDatabaseManager.FreeDatabase( database );
			}
			return( retValue );
		}

		public DbTrack GetTrack( long trackId ) {
			DbTrack	retValue = null;
			var database = mDatabaseManager.ReserveDatabase();

			try {
				var parms = database.Database.CreateParameters();

				parms["itemId"] = trackId;

				retValue = database.Database.ExecuteScalar( "SELECT DbTrack Where DbId = @itemId", parms ) as DbTrack;
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetTrack( trackId ): ", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public DataProviderList<DbTrack> GetTrackList( long albumId ) {
			DataProviderList<DbTrack>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			var parms = database.Database.CreateParameters();

			parms["albumId"] = albumId;

			try {
				retValue = new DataProviderList<DbTrack>( database.DatabaseId, FreeDatabase,
														  database.Database.ExecuteQuery( "SELECT DbTrack WHERE Album = @albumId", parms ).OfType<DbTrack>());
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetTrackList(albumId):", ex );

				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public DataProviderList<DbTrack> GetTrackList( DbAlbum forAlbum ) {
			Condition.Requires( forAlbum ).IsNotNull();

			return( GetTrackList( forAlbum.DbId ));
		}

		public DataProviderList<DbTrack> GetFavoriteTracks() {
			DataProviderList<DbTrack>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				retValue = new DataProviderList<DbTrack>( database.DatabaseId, FreeDatabase,
														  database.Database.ExecuteQuery( "SELECT DbTrack WHERE IsFavorite = true" ).OfType<DbTrack>());
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetFavoriteTracks: ", ex );

				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public DataProviderList<DbTrack> GetNewlyAddedTracks() {
			DataProviderList<DbTrack>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				retValue = new DataProviderList<DbTrack>( database.DatabaseId, FreeDatabase,
														  database.Database.ExecuteQuery( "SELECT DbTrack ORDER BY DateAddedTicks DESC" ).OfType<DbTrack>());
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetNewlyAddedTracks: ", ex );

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
				var parms = database.Database.CreateParameters();
				parms["trackId"] = trackId;

				Condition.Requires( trackId ).IsNotLessOrEqual( 0 );

				retValue = database.Database.ExecuteScalar( "SELECT StorageFile Where MetaDataPointer = @trackId", parms ) as StorageFile;
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetPhysicalFile:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public string GetPhysicalFilePath( StorageFile forFile ) {
			var retValue = "";
			var database = mDatabaseManager.ReserveDatabase();
			try {
				retValue = StorageHelpers.GetPath( database.Database, forFile );
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetPhysicalFilePath:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public void UpdateArtistInfo( long artistId ) {
			var artist = GetArtist( artistId );

			if( artist != null ) {
				UpdateArtistInfo( artist );
			}
		}

		public void UpdateArtistInfo( DbArtist forArtist ) {
			Condition.Requires( forArtist ).IsNotNull();

			mContentManager.RequestContent( forArtist );
		}

		public ArtistSupportInfo GetArtistSupportInfo( long artistId ) {
			return( GetArtistSupportInfo( GetArtist( artistId )));
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
				parms["bandMembers"] = ContentType.BandMembers;
				parms["biography"] = ContentType.Biography;
				parms["similarArtists"] = ContentType.SimilarArtists;
				parms["topAlbums"] = ContentType.TopAlbums;

				retValue = new ArtistSupportInfo( database.Database.ExecuteScalar( "SELECT DbTextInfo Where Artist = @artistId AND ContentType = @biography", parms ) as DbTextInfo,
												  database.Database.ExecuteScalar( "SELECT DbArtwork Where Artist = @artistId AND ContentType = @artistImage", parms ) as DbArtwork,
												  database.Database.ExecuteScalar( "SELECT DbAssociatedItemList Where Artist = @artistId AND ContentType = @similarArtists", parms ) as DbAssociatedItemList,
												  database.Database.ExecuteScalar( "SELECT DbAssociatedItemList Where Artist = @artistId AND ContentType = @topAlbums", parms ) as DbAssociatedItemList,
												  database.Database.ExecuteScalar( "SELECT DbAssociatedItemList Where Artist = @artistId AND ContentType = @bandMembers", parms ) as DbAssociatedItemList );
				
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetArtistSupportInfo:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public DataProviderList<DbDiscographyRelease> GetDiscography( long artistId ) {
			return( GetDiscography( GetArtist( artistId )));
		}

		public DataProviderList<DbDiscographyRelease> GetDiscography( DbArtist forArtist ) {
			Condition.Requires( forArtist ).IsNotNull();

			DataProviderList<DbDiscographyRelease>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				var parms = database.Database.CreateParameters();
				parms["artistId"] = forArtist.DbId;

				retValue = new DataProviderList<DbDiscographyRelease>( database.DatabaseId, FreeDatabase,
																	   database.Database.ExecuteQuery( "SELECT DbDiscographyRelease WHERE Artist = @artistId", parms ).OfType<DbDiscographyRelease>());
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetFavoriteTracks: ", ex );

				mDatabaseManager.FreeDatabase( database );
			}
			return( retValue );
		}


		public void UpdateAlbumInfo( DbAlbum forAlbum ) {
			Condition.Requires( forAlbum ).IsNotNull();

			mContentManager.RequestContent( forAlbum );
		}

		public AlbumSupportInfo GetAlbumSupportInfo( long albumId ) {
			AlbumSupportInfo	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				var parms = database.Database.CreateParameters();

				parms["albumId"] = albumId;
				parms["coverType"] = ContentType.AlbumCover;
				parms["otherType"] = ContentType.AlbumArtwork;

				retValue = new AlbumSupportInfo( database.Database.ExecuteQuery( "SELECT DbArtwork WHERE Album = @albumId AND ContentType = @coverType", parms ).OfType<DbArtwork>().ToArray(),
												 database.Database.ExecuteQuery( "SELECT DbArtwork WHERE Album = @albumId AND ContentType = @otherType", parms ).OfType<DbArtwork>().ToArray(),
												 database.Database.ExecuteQuery( "SELECT DbTextInfo WHERE Album = @albumId" ,parms ).OfType<DbTextInfo>().ToArray());
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetAlbumSupportInfo:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public AlbumSupportInfo GetAlbumSupportInfo( DbAlbum forAlbum ) {
			Condition.Requires( forAlbum ).IsNotNull();

			return( GetAlbumSupportInfo( forAlbum.DbId ));
		}

		public DbInternetStream GetStream( long streamId ) {
			DbInternetStream	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				var parms = database.Database.CreateParameters();

				parms["itemId"] = streamId;

				retValue = database.Database.ExecuteScalar( "SELECT DbInternetStream Where DbId = @itemId", parms ) as DbInternetStream;
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetStream:", ex );
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

		public DataUpdateShell<DbInternetStream> GetStreamForUpdate( long streamId ) {
			DataUpdateShell<DbInternetStream>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			if( database != null ) {
				var parms = database.Database.CreateParameters();

				parms["streamId"] = streamId;

				retValue = new DataUpdateShell<DbInternetStream>( database.DatabaseId, FreeDatabase, UpdateInternetStream,
																  database.Database.ExecuteScalar( "SELECT DbInternetStream Where DbId = @streamId", parms ) as DbInternetStream );
			}

			return( retValue );
		}

		private void UpdateInternetStream( string databaseId, DbInternetStream stream ) {
			var database = mDatabaseManager.GetDatabase( databaseId );

			if( database != null ) {
				database.Database.Store( stream );
			}
		}

		public DataProviderList<DbPlayList> GetPlayLists() {
			DataProviderList<DbPlayList>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				retValue = new DataProviderList<DbPlayList>( database.DatabaseId, FreeDatabase,
																from DbPlayList list in database.Database select list );
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetPlayLists:", ex );

				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public DataProviderList<DbGenre> GetGenreList() {
			DataProviderList<DbGenre>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				retValue = new DataProviderList<DbGenre>( database.DatabaseId, FreeDatabase,
																from DbGenre genre in database.Database select genre );
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetGenreList:", ex );

				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public DataProviderList<DbTrack> GetGenreTracks( long genreId ) {
			DataProviderList<DbTrack>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				retValue = new DataProviderList<DbTrack>( database.DatabaseId, FreeDatabase,
															from DbTrack track in database.Database where track.Genre == genreId select track );
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetGenreTracks:", ex );

				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}
	}
}
