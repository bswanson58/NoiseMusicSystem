using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Core.DataBuilders;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	public class DataProvider : IDataProvider {
		private readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEvents;
		private readonly IDatabaseManager	mDatabaseManager;
		private readonly IContentManager	mContentManager;
		private readonly ILog				mLog;

		public DataProvider( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();
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

		public DataProviderList<DbTrack> GetTrackList( DbAlbum forAlbum ) {
			Condition.Requires( forAlbum ).IsNotNull();

			DataProviderList<DbTrack>	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				var albumId = GetObjectIdentifier( forAlbum );

				retValue = new DataProviderList<DbTrack>( database.DatabaseId, FreeDatabase,
																from DbTrack track in database.Database where track.Album == albumId 
																orderby track.VolumeName, track.TrackNumber ascending select track );
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

		public AlbumSupportInfo GetAlbumSupportInfo( DbAlbum forAlbum ) {
			Condition.Requires( forAlbum ).IsNotNull();

			AlbumSupportInfo	retValue = null;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				var parms = database.Database.CreateParameters();
				var albumId = forAlbum.DbId;

				parms["albumId"] = forAlbum.DbId;
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

		public void SetFavorite( DbArtist forArtist, bool isFavorite ) {
			Condition.Requires( forArtist ).IsNotNull();

			var database = mDatabaseManager.ReserveDatabase();
			try {
				forArtist = database.ValidateOnThread( forArtist ) as DbArtist;
				if( forArtist != null ) {
					if( forArtist.IsFavorite != isFavorite ) {
						forArtist.IsFavorite = isFavorite;
						database.Store( forArtist );

						mEvents.GetEvent<Events.DatabaseItemChanged>().Publish( new DbItemChangedArgs( forArtist, DbItemChanged.Favorite ));
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - SetFavorite(DbArtist):", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}

		public void SetFavorite( DbAlbum forAlbum, bool isFavorite ) {
			Condition.Requires( forAlbum ).IsNotNull();

			var database = mDatabaseManager.ReserveDatabase();
			try {
				forAlbum = database.ValidateOnThread( forAlbum ) as DbAlbum;
				if( forAlbum != null ) {
					if( forAlbum.IsFavorite != isFavorite ) {
						forAlbum.IsFavorite = isFavorite;
						database.Store( forAlbum );

						mEvents.GetEvent<Events.DatabaseItemChanged>().Publish( new DbItemChangedArgs( forAlbum, DbItemChanged.Favorite ));
					}

					var artist = ( from DbArtist dbArtist in database.Database where dbArtist.DbId == forAlbum.Artist select dbArtist ).FirstOrDefault();
					if( artist != null ) {
						var albumList = from DbAlbum dbAlbum in database.Database where dbAlbum.Artist == artist.DbId select dbAlbum;

						artist.HasFavorites = false;

						if( albumList.Any( album => ( album.IsFavorite ) || ( album.HasFavorites ))) {
							artist.HasFavorites = true;
						}

						database.Store( artist );
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - SetFavorite(DbAlbum):", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}

		public void SetFavorite( DbTrack forTrack, bool isFavorite ) {
			Condition.Requires( forTrack ).IsNotNull();

			var database = mDatabaseManager.ReserveDatabase();
			try {
				forTrack = database.ValidateOnThread( forTrack ) as DbTrack;
				if( forTrack != null ) {
					if( forTrack.IsFavorite != isFavorite ) {
						forTrack.IsFavorite = isFavorite;
						database.Store( forTrack );

						mEvents.GetEvent<Events.DatabaseItemChanged>().Publish( new DbItemChangedArgs( forTrack, DbItemChanged.Favorite ));
					}

					var album = ( from DbAlbum dbAlbum in database.Database where dbAlbum.DbId == forTrack.Album select dbAlbum ).FirstOrDefault();
					if( album != null ) {
						var trackList = from DbTrack dbTrack in database.Database where dbTrack.Album == forTrack.Album select dbTrack;

						album.HasFavorites = false;

						if( trackList.Any( t => ( t.IsFavorite ))) {
							album.HasFavorites = true;
						}

						database.Store( album );

						var artist = ( from DbArtist dbArtist in database.Database where dbArtist.DbId == album.Artist select dbArtist ).FirstOrDefault();
						if( artist != null ) {
							var albumList = from DbAlbum dbAlbum in database.Database where dbAlbum.Artist == album.Artist select dbAlbum;

							artist.HasFavorites = false;

							if( albumList.Any( a => ( a.IsFavorite ) || ( a.HasFavorites ))) {
								artist.HasFavorites = true;
							}

							database.Store( artist );
						}
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - SetFavorite(DbTrack):", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}

		public void SetFavorite( DbPlayList forList, bool isFavorite ) {
			Condition.Requires( forList ).IsNotNull();

			var database = mDatabaseManager.ReserveDatabase();
			try {
				forList = database.ValidateOnThread( forList ) as DbPlayList;
				if( forList != null ) {
					if( forList.IsFavorite != isFavorite ) {
						forList.IsFavorite = isFavorite;
						database.Store( forList );

						mEvents.GetEvent<Events.DatabaseItemChanged>().Publish( new DbItemChangedArgs( forList, DbItemChanged.Favorite ));
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - SetFavorite(DbList):", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}

		public void SetRating( DbArtist forArtist, Int16 rating ) {
			Condition.Requires( forArtist ).IsNotNull();

			var database = mDatabaseManager.ReserveDatabase();
			try {
				forArtist = database.ValidateOnThread( forArtist ) as DbArtist;
				if( forArtist != null ) {
					forArtist.UserRating = rating;

					database.Store( forArtist );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - SetRating(DbArtist):", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}

		public void SetRating( DbAlbum forAlbum, Int16 rating ) {
			Condition.Requires( forAlbum ).IsNotNull();

			var database = mDatabaseManager.ReserveDatabase();
			try {
				forAlbum = database.ValidateOnThread( forAlbum ) as DbAlbum;
				if( forAlbum != null ) {
					forAlbum.UserRating = rating;
					database.Store( forAlbum );

					var artist = ( from DbArtist dbArtist in database.Database where dbArtist.DbId == forAlbum.Artist select dbArtist ).FirstOrDefault();
					if( artist != null ) {
						var albumList = from DbAlbum dbAlbum in database.Database where dbAlbum.Artist == artist.DbId select dbAlbum;
						var maxAlbumRating = 0;

						foreach( var album in albumList ) {
							if( album.Rating > maxAlbumRating ) {
								maxAlbumRating = album.Rating;
							}
							if( album.MaxChildRating > maxAlbumRating ) {
								maxAlbumRating = album.MaxChildRating;
							}
						}

						artist.MaxChildRating = (Int16)maxAlbumRating;
						database.Store( artist );
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - SetRating(DbAlbum):", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}

		public void SetRating( DbTrack forTrack, Int16 rating ) {
			Condition.Requires( forTrack ).IsNotNull();

			var database = mDatabaseManager.ReserveDatabase();
			try {
				forTrack = database.ValidateOnThread( forTrack ) as DbTrack;
				if( forTrack != null ) {
					forTrack.Rating = rating;
					database.Store( forTrack );

					var album = ( from DbAlbum dbAlbum in database.Database where dbAlbum.DbId == forTrack.Album select dbAlbum ).FirstOrDefault();
					if( album != null ) {
						var trackList = from DbTrack dbTrack in database.Database where dbTrack.Album == forTrack.Album select dbTrack;
						var maxTrackRating = 0;

						foreach( var track in trackList ) {
							if( track.Rating > maxTrackRating ) {
								maxTrackRating = track.Rating;
							}
						}
						album.MaxChildRating = (Int16)maxTrackRating;
						database.Store( album );

						var artist = ( from DbArtist dbArtist in database.Database where dbArtist.DbId == album.Artist select dbArtist ).FirstOrDefault();
						if( artist != null ) {
							var albumList = from DbAlbum dbAlbum in database.Database where dbAlbum.Artist == album.Artist select dbAlbum;
							var maxAlbumRating = 0;

							foreach( var a in albumList ) {
								if( a.Rating > maxAlbumRating ) {
									maxAlbumRating = a.Rating;
								}
								if( a.MaxChildRating > maxAlbumRating ) {
									maxAlbumRating = a.MaxChildRating;
								}
							}

							artist.MaxChildRating = (Int16)maxAlbumRating;
							database.Store( artist );
						}
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - SetRating(DbTrack):", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}

		public void SetRating( DbPlayList forList, Int16 rating ) {
			Condition.Requires( forList ).IsNotNull();

			var database = mDatabaseManager.ReserveDatabase();
			try {
				forList = database.ValidateOnThread( forList ) as DbPlayList;
				if( forList != null ) {
					forList.Rating = rating;

					database.Store( forList );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - SetRating(DbPlayList):", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}

	}
}
