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

		public void UpdateItem( object item ) {
			Condition.Requires( item ).IsNotNull();

			if(( item is DbArtist ) ||
			   ( item is DbAlbum ) ||
			   ( item is DbTrack ) ||
			   ( item is DbGenre ) ||
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

		public DbArtist GetArtist( long dbid ) {
			var			database = mDatabaseManager.ReserveDatabase();
			DbArtist	retValue = null;

			try {
				retValue = ( from DbArtist artist in database.Database where artist.DbId == dbid select artist ).FirstOrDefault();
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

		public DataProviderList<DbArtist> GetFavoriteArtists() {
			DataProviderList<DbArtist>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				retValue = new DataProviderList<DbArtist>( database.DatabaseId, FreeDatabase,
															from DbArtist artist in database.Database where artist.IsFavorite select artist );				
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
				retValue = ( from DbAlbum album in database.Database where album.DbId == dbid select album ).FirstOrDefault();
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

			try {
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

		public DataProviderList<DbAlbum> GetFavoriteAlbums() {
			DataProviderList<DbAlbum>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				retValue = new DataProviderList<DbAlbum>( database.DatabaseId, FreeDatabase,
															from DbAlbum album in database.Database where album.IsFavorite select album );				
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetFavoriteAlbums: ", ex );

				mDatabaseManager.FreeDatabase( database );
			}
			return( retValue );
		}

		public DbTrack GetTrack( StorageFile forFile ) {
			Condition.Requires( forFile ).IsNotNull();

			DbTrack	retValue = null;
			var database = mDatabaseManager.ReserveDatabase();

			try {
				retValue = ( from DbTrack track in database.Database where forFile.MetaDataPointer == track.DbId select track ).FirstOrDefault();
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - GetTrack( StorageFile ): ", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public DbTrack GetTrack( long trackId ) {
			DbTrack	retValue = null;
			var database = mDatabaseManager.ReserveDatabase();

			try {
				retValue = ( from DbTrack track in database.Database where track.DbId == trackId select track ).FirstOrDefault();
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
			try {
				retValue = new DataProviderList<DbTrack>( database.DatabaseId, FreeDatabase,
																from DbTrack track in database.Database where track.Album == albumId 
																orderby track.VolumeName, track.TrackNumber ascending select track );
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

		public DataProviderList<DbTrack> GetFavoriteTracks() {
			DataProviderList<DbTrack>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				retValue = new DataProviderList<DbTrack>( database.DatabaseId, FreeDatabase,
															from DbTrack track in database.Database where track.IsFavorite select track );				
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
															from DbTrack track in database.Database orderby track.DateAdded descending select track );				
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

				retValue = new ArtistSupportInfo(( from DbTextInfo bio in database.Database where bio.Artist == artistId && bio.ContentType == ContentType.Biography select bio ).FirstOrDefault(),
												   database.Database.ExecuteScalar( "SELECT DbArtwork Where Artist = @artistId AND ContentType = @artistImage", parms ) as DbArtwork,
												 ( from DbAssociatedItemList item in database.Database where item.Artist == artistId && item.ContentType == ContentType.SimilarArtists select item ).FirstOrDefault(),
												 ( from DbAssociatedItemList item in database.Database where item.Artist == artistId && item.ContentType == ContentType.TopAlbums select item ).FirstOrDefault(),
												 ( from DbAssociatedItemList item in database.Database where item.Artist == artistId && item.ContentType == ContentType.BandMembers select item ).FirstOrDefault());
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
				var artistId = forArtist.DbId;

				retValue = new DataProviderList<DbDiscographyRelease>( database.DatabaseId, FreeDatabase,
															from DbDiscographyRelease release in database.Database where release.Artist == artistId select release );				
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
												( from DbTextInfo info in database.Database where info.Album == albumId select info ).ToArray());
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
				retValue = ( from DbInternetStream stream in database.Database where stream.DbId == streamId select stream ).FirstOrDefault();
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
