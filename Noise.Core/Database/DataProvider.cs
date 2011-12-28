using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CuttingEdge.Conditions;
using Microsoft.Practices.Prism.Events;
using Noise.Core.FileStore;
using Noise.Core.Support;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.Database {
	public class DataProvider : IDataProvider, IRequireConstruction {
		private readonly IEventAggregator	mEvents;
		private readonly IDatabaseManager	mDatabaseManager;
		private long						mDatabaseId;

		public DataProvider( IEventAggregator eventAggregator, IDatabaseManager databaseManager ) {
			mEvents = eventAggregator;
			mDatabaseManager = databaseManager;
			mDatabaseId = Constants.cDatabaseNullOid;

			NoiseLogger.Current.LogInfo( "DataProvider created" );
		}

		public long DatabaseId {
			get {
				if( mDatabaseId == Constants.cDatabaseNullOid ) {
					IDatabase database = null;

					try {
						database = mDatabaseManager.ReserveDatabase();

						if( database != null ) {
							mDatabaseId = database.DatabaseVersion.DatabaseId;
						}
					}
					catch( Exception ex ) {
						NoiseLogger.Current.LogException( "Exception - Could not access database id.", ex );
					}
					finally {
						if( database != null ) {
							mDatabaseManager.FreeDatabase( database );
						}
					}
				}
				return( mDatabaseId );
			}
		}

		private void FreeDatabase( string databaseId ) {
			mDatabaseManager.FreeDatabase( databaseId );
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
				NoiseLogger.Current.LogException( "Exception - GetItem:", ex );
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
			   ( item is DbTag ) ||
			   ( item is DbTagAssociation ) ||
			   ( item is DbInternetStream )) {
				var database = mDatabaseManager.ReserveDatabase();

				try {
					database.Insert( item );
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Exception - InsertItem:", ex );
				}
				finally {
					mDatabaseManager.FreeDatabase( database );
				}
			}
			else {
				NoiseLogger.Current.LogMessage( string.Format( "InsertItem: unknown item type: {0}", item.GetType()));
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
					NoiseLogger.Current.LogException( "Exception - UpdateItem:", ex );
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
				NoiseLogger.Current.LogException( "Exception - GetArtist:", ex );
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
				NoiseLogger.Current.LogException( ex );

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
					NoiseLogger.Current.LogException( ex );

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
				NoiseLogger.Current.LogException( "Exception - GetArtistForAlbum:", ex );
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
				NoiseLogger.Current.LogException( "Exception - GetFavoriteArtists: ", ex );

				mDatabaseManager.FreeDatabase( database );
			}
			return( retValue );
		}

		public DataUpdateShell<DbArtist> GetArtistForUpdate( long artistId ) {
			DataUpdateShell<DbArtist>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			if( database != null ) {
				var parms = database.Database.CreateParameters();

				parms["artistId"] = artistId;

				retValue = new DataUpdateShell<DbArtist>( database.DatabaseId, FreeDatabase, UpdateArtist,
														  database.Database.ExecuteScalar( "SELECT DbArtist Where DbId = @artistId", parms ) as DbArtist );
			}

			return( retValue );
		}


		private void UpdateArtist( string databaseId, DbArtist artist ) {
			var database = mDatabaseManager.GetDatabase( databaseId );

			if( database != null ) {
				artist.UpdateLastChange();

				database.Store( artist );
			}
		}

		public void	UpdateArtistLastChanged( long artistId ) {
			var	database = mDatabaseManager.ReserveDatabase();

			try {
				var parms = database.Database.CreateParameters();

				parms["artistId"] = artistId;

				var artist = database.Database.ExecuteScalar( "SELECT DbArtist Where DbId = @artistId", parms ) as DbArtist;

				if( artist != null ) {
					artist.UpdateLastChange();

					database.Database.Store( artist );
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - UpdateArtistLastChanged:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
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
				NoiseLogger.Current.LogException( "Exception - GetAlbum:", ex );
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
				NoiseLogger.Current.LogException( "Exception - GetAlbumList:", ex );

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
				NoiseLogger.Current.LogException( "Exception - GetAlbumForTrack:", ex );
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
				NoiseLogger.Current.LogException( "Exception - GetFavoriteAlbums: ", ex );

				mDatabaseManager.FreeDatabase( database );
			}
			return( retValue );
		}

		public DataUpdateShell<DbAlbum> GetAlbumForUpdate( long albumId ) {
			DataUpdateShell<DbAlbum>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			if( database != null ) {
				var parms = database.Database.CreateParameters();

				parms["albumId"] = albumId;

				retValue = new DataUpdateShell<DbAlbum>( database.DatabaseId, FreeDatabase, UpdateAlbum,
														 database.Database.ExecuteScalar( "SELECT DbAlbum Where DbId = @albumId", parms ) as DbAlbum );
			}

			return( retValue );
		}


		private void UpdateAlbum( string databaseId, DbAlbum album ) {
			var database = mDatabaseManager.GetDatabase( databaseId );

			if( database != null ) {
				database.Store( album );

				UpdateArtistLastChanged( album.Artist );
			}
		}

		public DataUpdateShell<DbArtwork> GetArtworkForUpdate( long artworkId ) {
			DataUpdateShell<DbArtwork>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			if( database != null ) {
				var parms = database.Database.CreateParameters();

				parms["artworkId"] = artworkId;

				retValue = new DataUpdateShell<DbArtwork>( database.DatabaseId, FreeDatabase, UpdateArtwork,
														   database.Database.ExecuteScalar( "SELECT DbArtwork Where DbId = @artworkId", parms ) as DbArtwork );
			}

			return( retValue );
		}


		private void UpdateArtwork( string databaseId, DbArtwork artwork ) {
			var database = mDatabaseManager.GetDatabase( databaseId );

			if( database != null ) {
				database.Store( artwork );
			}
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
				NoiseLogger.Current.LogException( "Exception - GetTrack( trackId ): ", ex );
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
				NoiseLogger.Current.LogException( "Exception - GetTrackList(albumId):", ex );

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
				NoiseLogger.Current.LogException( "Exception - GetFavoriteTracks: ", ex );

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
				NoiseLogger.Current.LogException( "Exception - GetNewlyAddedTracks: ", ex );

				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public DataUpdateShell<DbTrack> GetTrackForUpdate( long trackId ) {
			DataUpdateShell<DbTrack>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			if( database != null ) {
				var parms = database.Database.CreateParameters();

				parms["trackId"] = trackId;

				retValue = new DataUpdateShell<DbTrack>( database.DatabaseId, FreeDatabase, UpdateTrack,
														 database.Database.ExecuteScalar( "SELECT DbTrack Where DbId = @trackId", parms ) as DbTrack );
			}

			return( retValue );
		}


		private void UpdateTrack( string databaseId, DbTrack track ) {
			var database = mDatabaseManager.GetDatabase( databaseId );

			if( database != null ) {
				database.Store( track );

				var album = GetAlbum( track.Album );

				if( album != null ) {
					UpdateArtistLastChanged( album.Artist );
				}
			}
		}

		public StorageFile GetPhysicalFile( DbTrack forTrack ) {
			Condition.Requires( forTrack ).IsNotNull();

			StorageFile	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				var parms = database.Database.CreateParameters();

				parms["trackId"] = forTrack.DbId;

				retValue = database.Database.ExecuteScalar( "SELECT StorageFile Where MetaDataPointer = @trackId", parms ) as StorageFile;
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - GetPhysicalFile:", ex );
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
//				retValue = StorageHelpers.GetPath( database.Database, forFile );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - GetPhysicalFilePath:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public string GetAlbumPath( long albumId ) {
			var retValue = "";
			var database = mDatabaseManager.ReserveDatabase();

			try {
				var album = GetAlbum( albumId );

				if( album != null ) {
					using( var albumTracks = GetTrackList( album )) {
						var trackList = albumTracks.List.Select( GetPhysicalFile );
						var parentList = trackList.Select( track => track.ParentFolder ).Distinct();
						var folderList = parentList.Select( GetStorageFolder );
//						var pathList = folderList.Select( folder => StorageHelpers.GetPath( database.Database, folder ));

//						retValue = FindCommonParent( pathList );
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - GetAlbumPath:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		private static string FindCommonParent( IEnumerable<string> paths ) {
			var retValue = "";
			var pathList = paths.Where( path => !string.IsNullOrWhiteSpace( path )).ToList();

			if( pathList.Count() > 0 ) {
				if( pathList.Count() == 1 ) {
					retValue = pathList.First();
				}
				else {
					var match = true;
					var index = 0;

					retValue = pathList.First().Substring( 0, index + 1 );

					while(( match ) &&
						  ( index < pathList.First().Length )) {
						var matchString = retValue;

						match = pathList.All( path => path.StartsWith( matchString ));
						index++;

						if(( match ) &&
						   ( index < pathList.First().Length )) {
							retValue = pathList.First().Substring( 0, index + 1 );
						}
					}

					if(!match ) {
						var lastSlash = retValue.LastIndexOfAny( new [] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar } );

						if( lastSlash > 0 ) {
							retValue = retValue.Substring( 0, lastSlash + 1 );
						}
					}
				}
			}

			return( retValue );
		}

		private StorageFolder GetStorageFolder( long folderId ) {
			StorageFolder	retValue = null;
			var database = mDatabaseManager.ReserveDatabase();

			try {
				var parms = database.Database.CreateParameters();

				parms["folderId"] = folderId;

				retValue = database.Database.ExecuteScalar( "SELECT StorageFolder WHERE DbId = @folderId", parms ) as StorageFolder;
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - GetStorageFolder:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public void UpdateArtistInfo( long artistId ) {
			var artist = GetArtist( artistId );

			if( artist != null ) {
				mEvents.GetEvent<Events.ArtistContentRequested>().Publish( artist );
			}
		}

		public ArtistSupportInfo GetArtistSupportInfo( long artistId ) {
			ArtistSupportInfo	retValue = null;
			var database = mDatabaseManager.ReserveDatabase();
			try {
				var parms = database.Database.CreateParameters();

				parms["artistId"] = artistId;
				parms["artistImage"] = ContentType.ArtistPrimaryImage;
				parms["bandMembers"] = ContentType.BandMembers;
				parms["biography"] = ContentType.Biography;
				parms["similarArtists"] = ContentType.SimilarArtists;
				parms["topAlbums"] = ContentType.TopAlbums;

				var similarArtists = database.Database.ExecuteScalar( "SELECT DbAssociatedItemList Where Artist = @artistId AND ContentType = @similarArtists", parms ) as DbAssociatedItemList;
				var topAlbums = database.Database.ExecuteScalar( "SELECT DbAssociatedItemList Where Artist = @artistId AND ContentType = @topAlbums", parms ) as DbAssociatedItemList;
				var bandMembers = database.Database.ExecuteScalar( "SELECT DbAssociatedItemList Where Artist = @artistId AND ContentType = @bandMembers", parms ) as DbAssociatedItemList;

				Artwork	artwork = null;
				var artistImage = database.Database.ExecuteScalar( "SELECT DbArtwork Where Artist = @artistId AND ContentType = @artistImage", parms ) as DbArtwork;
				if( artistImage != null ) {
					artwork = TransformArtwork( artistImage, database );
				}

				TextInfo textInfo = null;
				var	dbTextInfo =  database.Database.ExecuteScalar( "SELECT DbTextInfo Where Artist = @artistId AND ContentType = @biography", parms ) as DbTextInfo;
				if( dbTextInfo != null ) {
					textInfo = TransformTextInfo( dbTextInfo, database );
				}

				retValue = new ArtistSupportInfo( textInfo, artwork, similarArtists, topAlbums, bandMembers );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - GetArtistSupportInfo:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public DataProviderList<DbDiscographyRelease> GetDiscography( long artistId ) {
			DataProviderList<DbDiscographyRelease>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				var parms = database.Database.CreateParameters();
				parms["artistId"] = artistId;

				retValue = new DataProviderList<DbDiscographyRelease>( database.DatabaseId, FreeDatabase,
																	   database.Database.ExecuteQuery( "SELECT DbDiscographyRelease WHERE Artist = @artistId", parms ).OfType<DbDiscographyRelease>());
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - GetDiscography: ", ex );

				mDatabaseManager.FreeDatabase( database );
			}
			return( retValue );
		}

		private static Artwork TransformArtwork( DbArtwork artwork, IDatabase database ) {
			return( new Artwork( artwork ) { Image = database.BlobStorage.RetrieveBytes( artwork.DbId ) });
		}

		private static TextInfo TransformTextInfo( DbTextInfo textInfo, IDatabase database ) {
			return( new TextInfo( textInfo ) { Text = database.BlobStorage.RetrieveText( textInfo.DbId )});
		}

		public AlbumSupportInfo GetAlbumSupportInfo( long albumId ) {
			AlbumSupportInfo	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				var parms = database.Database.CreateParameters();

				parms["albumId"] = albumId;
				parms["coverType"] = ContentType.AlbumCover;
				parms["otherType"] = ContentType.AlbumArtwork;

				var dbAlbumCovers = database.Database.ExecuteQuery( "SELECT DbArtwork WHERE Album = @albumId AND ( ContentType = @coverType OR IsUserSelection )", parms ).OfType<DbArtwork>().ToArray();
				var dbAlbumArtwork =  database.Database.ExecuteQuery( "SELECT DbArtwork WHERE Album = @albumId AND ContentType = @otherType", parms ).OfType<DbArtwork>().ToArray();
				var textInfo = database.Database.ExecuteQuery( "SELECT DbTextInfo WHERE Album = @albumId" ,parms ).OfType<DbTextInfo>().ToArray();

				retValue = new AlbumSupportInfo( dbAlbumCovers.Select( dbAlbum => TransformArtwork( dbAlbum, database ) ).ToArray(),
												 dbAlbumArtwork.Select( dbAlbum => TransformArtwork( dbAlbum, database ) ).ToArray(),
												 textInfo.Select( info => TransformTextInfo( info, database )).ToArray());
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - GetAlbumSupportInfo:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public void	StoreLyric( DbLyric lyric ) {
			var database = mDatabaseManager.ReserveDatabase();

			try {
				var parms = database.Database.CreateParameters();

				parms["artistId"] = lyric.ArtistId;
				parms["trackId"] = lyric.TrackId;

				var match = database.Database.ExecuteScalar( "SELECT DbLyric WHERE ArtistId = @artistId AND TrackId = @trackId", parms ) as DbLyric;
				if( match != null ) {
					database.Delete( match );
				}

				database.Insert( lyric );

				UpdateArtistLastChanged( lyric.ArtistId );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - StoreLyric:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}

		public DataProviderList<DbLyric> GetPossibleLyrics( DbArtist artist, DbTrack track ) {
			DataProviderList<DbLyric>	retValue = null;
			var	list = new List<DbLyric>();
			var	database = mDatabaseManager.ReserveDatabase();

			try {
				var parms = database.Database.CreateParameters();

				parms["artistId"] = artist.DbId;
				parms["trackId"] = track.DbId;
				parms["songName"] = track.Name;

				var match = database.Database.ExecuteScalar( "SELECT DbLyric WHERE ArtistId = @artistId AND TrackId = @trackId", parms ) as DbLyric;
				if( match != null ) {
					list.Add( match );
				}

				var lyricsList = database.Database.ExecuteQuery( "SELECT DbLyric WHERE ArtistId = @artistId AND SongName = @songName", parms ).OfType<DbLyric>();
				list.AddRange( lyricsList );

				lyricsList = database.Database.ExecuteQuery( "SELECT DbLyric WHERE SongName = @songName", parms ).OfType<DbLyric>();
				list.AddRange( lyricsList );

				retValue = new DataProviderList<DbLyric>( database.DatabaseId, FreeDatabase, lyricsList );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - GetPossibleLyrics:", ex );

				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public DataUpdateShell<DbLyric> GetLyricForUpdate( long lyricId ) {
			DataUpdateShell<DbLyric>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			if( database != null ) {
				var parms = database.Database.CreateParameters();

				parms["lyricId"] = lyricId;

				retValue = new DataUpdateShell<DbLyric>( database.DatabaseId, FreeDatabase, UpdateLyric,
														 database.Database.ExecuteScalar( "SELECT DbLyric Where DbId = @lyricId", parms ) as DbLyric );
			}

			return( retValue );
		}


		private void UpdateLyric( string databaseId, DbLyric lyric ) {
			var database = mDatabaseManager.GetDatabase( databaseId );

			if( database != null ) {
				database.Database.Store( lyric );

				UpdateArtistLastChanged( lyric.ArtistId );
			}
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
				NoiseLogger.Current.LogException( "Exception - GetStream:", ex );
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
				NoiseLogger.Current.LogException( "Exception - GetStreamList:", ex );

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
				NoiseLogger.Current.LogException( "Exception - GetPlayLists:", ex );

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
				NoiseLogger.Current.LogException( "Exception - GetGenreList:", ex );

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
				NoiseLogger.Current.LogException( "Exception - GetGenreTracks:", ex );

				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public DataProviderList<DbTag> GetTagList( eTagGroup group ) {
			DataProviderList<DbTag>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				var parms = database.Database.CreateParameters();

				parms["group"] = group;

				retValue = new DataProviderList<DbTag>( database.DatabaseId, FreeDatabase,
														database.Database.ExecuteQuery( "SELECT DbTag Where TagGroup = @group", parms ).OfType<DbTag>());
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - GetTagList:", ex );

				mDatabaseManager.FreeDatabase( database );
			}
			return( retValue );
		}

		public DataProviderList<DbTagAssociation> GetTagAssociations( long tagId ) {
			DataProviderList<DbTagAssociation>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				var parms = database.Database.CreateParameters();

				parms["tagId"] = tagId;

				retValue = new DataProviderList<DbTagAssociation>( database.DatabaseId, FreeDatabase,
																   database.Database.ExecuteQuery( "SELECT DbTagAssociation Where TagId = @tagId", parms ).OfType<DbTagAssociation>());
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - GetTagList:", ex );

				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public DataProviderList<long> GetArtistCategories( long artistId ) {
			DataProviderList<long>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				var parms = database.Database.CreateParameters();

				parms["artistId"] = artistId;
				parms["group"] = eTagGroup.User;

				var tagList = database.Database.ExecuteQuery( "SELECT DbTagAssociation Where TagGroup = @group AND ArtistId = @artistId", parms ).OfType<DbTagAssociation>();
				retValue = new DataProviderList<long>( database.DatabaseId, FreeDatabase, from assoc in tagList select assoc.TagId );
				
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - GetArtistCategories:", ex );

				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public DataProviderList<long> GetAlbumCategories( long albumId ) {
			DataProviderList<long>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				var parms = database.Database.CreateParameters();

				parms["albumId"] = albumId;
				parms["group"] = eTagGroup.User;

				var tagList = database.Database.ExecuteQuery( "SELECT DbTagAssociation Where TagGroup = @group AND AlbumId = @albumId", parms ).OfType<DbTagAssociation>();
				retValue = new DataProviderList<long>( database.DatabaseId, FreeDatabase, from assoc in tagList select assoc.TagId );
				
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - GetAlbumCategories:", ex );

				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public void SetAlbumCategories( long artistId, long albumId, IEnumerable<long> categories ) {
			var currentCategories = GetAlbumCategories( albumId );
			var removeList = currentCategories.List.Where( tagId => !categories.Contains( tagId )).ToList();
			var addList = categories.Where( tagId => !currentCategories.List.Contains( tagId )).ToList();

			if(( removeList.Count > 0 ) ||
			   ( addList.Count > 0 )) {
				var database = mDatabaseManager.ReserveDatabase();
				var parms = database.Database.CreateParameters();

				parms["group"] = eTagGroup.User;
				parms["albumId"] = albumId;

				try {
					foreach( var tagId in removeList ) {
						parms["tagId"] = tagId;

						var tagAssoc = database.Database.ExecuteScalar( "SELECT DbTagAssociation Where TagGroup = @group AND AlbumId = @albumId AND TagId = @tagId", parms ) as DbTagAssociation;
						if( tagAssoc != null ) {
							database.Database.Delete( tagAssoc );
						}
					}

					foreach( var tagId in addList ) {
						InsertItem( new DbTagAssociation( eTagGroup.User, tagId, artistId, albumId ));
					}
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Exception - SetAlbumCategories", ex );
				}
				finally {
					mDatabaseManager.FreeDatabase( database );
				}
			}
		}

		public DataProviderList<long> GetAlbumsInCategory( long categoryId ) {
			DataProviderList<long>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				var parms = database.Database.CreateParameters();

				parms["tagId"] = categoryId;
				parms["group"] = eTagGroup.User;

				var tagList = database.Database.ExecuteQuery( "SELECT DbTagAssociation Where TagGroup = @group AND TagId = @tagId", parms ).OfType<DbTagAssociation>();

				retValue = new DataProviderList<long>( database.DatabaseId, FreeDatabase, from assoc in tagList select assoc.AlbumId );
				
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - GetAlbumCategories:", ex );

				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}


		public DataFindResults Find( string artist, string album, string track ) {
			DataFindResults	retValue = null;

			try {
				if(!string.IsNullOrWhiteSpace( artist )) {
					using( var artistList = GetArtistList()) {
						var dbArtist = ( from DbArtist a in artistList.List 
											where a.Name.Equals( artist, StringComparison.CurrentCultureIgnoreCase ) select a ).FirstOrDefault();
						if( dbArtist != null ) {
							if(!string.IsNullOrEmpty( album )) {
								using( var albumList = GetAlbumList( dbArtist )) {
									var dbAlbum = ( from DbAlbum a in albumList.List
													where a.Name.Equals( album, StringComparison.CurrentCultureIgnoreCase ) select a ).FirstOrDefault();
									if( dbAlbum != null ) {
										if(!string.IsNullOrWhiteSpace( track )) {
											using( var trackList = GetTrackList( dbAlbum )) {
												var dbTrack = ( from DbTrack t in trackList.List
																where t.Name.Equals( track, StringComparison.CurrentCultureIgnoreCase ) select t ).FirstOrDefault();
												retValue = dbTrack != null ? new DataFindResults( DatabaseId, dbArtist, dbAlbum, dbTrack, true ) :
																			 new DataFindResults( DatabaseId, dbArtist, dbAlbum, false );
											}
										}
										else {
											retValue = new DataFindResults( DatabaseId, dbArtist, dbAlbum, true );
										}
									}
									else {
										retValue = new DataFindResults( DatabaseId, dbArtist, false );
									}
								}
							}
							else {
								retValue = new DataFindResults( DatabaseId, dbArtist, true );
							}
						}
					}
				}		
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - DataProvider:Find: ", ex );
			}

			return( retValue );
		}

		public DataFindResults Find( long itemId ) {
			DataFindResults	retValue = null;
			var item = GetItem( itemId );

			if( item != null ) {
				TypeSwitch.Do( item, TypeSwitch.Case<DbArtist>( artist => retValue = new DataFindResults( DatabaseId, artist, true )),
									 TypeSwitch.Case<DbAlbum>( album => {
									                           		var artist = GetArtistForAlbum( album );
																	if( artist != null ) {
																		retValue = new DataFindResults( DatabaseId, GetArtistForAlbum( album ), album, true );
																	}
																}),
									 TypeSwitch.Case<DbTrack>( track => {
									                           		var album = GetAlbumForTrack( track );
																	if( album != null ) {
																		var artist = GetArtistForAlbum( album );
																		if( artist != null ) {
																			retValue = new DataFindResults( DatabaseId, artist, album, track, true );
																		}
																	}
									                           }));
			}

			return( retValue );
		}

		public long	GetTimestamp( string componentId ) {
			var retValue = 0L;
			var database = mDatabaseManager.ReserveDatabase();

			try {
				var parms = database.Database.CreateParameters();
				parms["id"] = componentId;

				var timestamp = database.Database.ExecuteScalar( "SELECT DbTimestamp WHERE ComponentId = @id", parms ) as DbTimestamp;

				if( timestamp != null ) {
					retValue = timestamp.Timestamp;
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( string.Format( "Exception - GetTimestamp: {0}", componentId ), ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}

			return( retValue );
		}

		public void SetTimestamp( string componentId, long ticks ) {
			var database = mDatabaseManager.ReserveDatabase();

			try {
				var	parms = database.Database.CreateParameters();
				parms["id"] = componentId;

				var	timestamp = database.Database.ExecuteScalar( "SELECT DbTimestamp WHERE ComponentId = @id", parms ) as DbTimestamp;

				if( timestamp != null ) {
					timestamp.SetTimestamp( ticks );

					database.Store( timestamp );
				}
				else {
					timestamp = new DbTimestamp( componentId );
					timestamp.SetTimestamp( ticks );

					database.Insert( timestamp );
				}
				
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( string.Format( "Exception - UpdateTimestamp: {0}", componentId ), ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}
	}
}
