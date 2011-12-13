using System;
using System.Linq;
using CuttingEdge.Conditions;
using Microsoft.Practices.Prism.Events;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.Database {
	internal class DataUpdates : IDataUpdates {
		private readonly IEventAggregator	mEvents;
		private readonly IDatabaseManager	mDatabaseManager;

		private AsyncCommand<SetFavoriteCommandArgs>	mSetFavoriteCommand;
		private AsyncCommand<SetRatingCommandArgs>		mSetRatingCommand;
		private AsyncCommand<SetAlbumCoverCommandArgs>	mSetAlbumCoverCommand;

		public DataUpdates( IEventAggregator eventAggregator, IDatabaseManager databaseManager ) {
			mEvents = eventAggregator;
			mDatabaseManager = databaseManager;

			NoiseLogger.Current.LogInfo( "DataUpdates created." );
		}

		public bool Initialize() {
			mSetFavoriteCommand = new AsyncCommand<SetFavoriteCommandArgs>( OnSetFavorite );
			mSetFavoriteCommand.ExecutionComplete += OnExecutionComplete;
			GlobalCommands.SetFavorite.RegisterCommand( mSetFavoriteCommand );

			mSetRatingCommand = new AsyncCommand<SetRatingCommandArgs>( OnSetRating );
			mSetRatingCommand.ExecutionComplete += OnExecutionComplete;
			GlobalCommands.SetRating.RegisterCommand( mSetRatingCommand );

			mSetAlbumCoverCommand = new AsyncCommand<SetAlbumCoverCommandArgs>( OnSetAlbumCover );
			mSetAlbumCoverCommand.ExecutionComplete += OnExecutionComplete;
			GlobalCommands.SetAlbumCover.RegisterCommand( mSetAlbumCoverCommand );

			return( true );
		}

		private void OnExecutionComplete( object sender, AsyncCommandCompleteEventArgs args ) {
			if(( args != null ) &&
			   ( args.Exception != null )) {
				NoiseLogger.Current.LogException( "Exception - DataUpdates:OnExecutionComplete:", args.Exception );
			}
		}

		private void OnSetFavorite( SetFavoriteCommandArgs args ) {
			Condition.Requires( args ).IsNotNull();

			var database = mDatabaseManager.ReserveDatabase();

			try {
				var	parms = database.Database.CreateParameters();
				parms["dbid"] = args.ItemId;

				var item = database.Database.ExecuteScalar( "SELECT DbBase WHERE DbId = @dbid", parms );

				if( item != null ) {
					TypeSwitch.Do( item, TypeSwitch.Case<DbArtist>( artist => SetFavorite( database, artist, args.Value )),
										 TypeSwitch.Case<DbAlbum>( album => SetFavorite( database, album, args.Value  )),
										 TypeSwitch.Case<DbTrack>( track => SetFavorite( database, track, args.Value )),
										 TypeSwitch.Case<DbPlayList>( playList => SetFavorite( database, playList, args.Value )),
										 TypeSwitch.Default( () => NoiseLogger.Current.LogMessage( String.Format( "Unknown type passed to SetFavorite: {0}", item.GetType()))));
				}
				else {
					NoiseLogger.Current.LogMessage( String.Format( "Cannot locate item for SetFavoriteCommand: {0}", args.ItemId ) );
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - SetFavorite:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}

		private void OnSetRating( SetRatingCommandArgs args ) {
			Condition.Requires( args ).IsNotNull();

			var database = mDatabaseManager.ReserveDatabase();

			try {
				var	parms = database.Database.CreateParameters();
				parms["dbid"] = args.ItemId;

				var item = database.Database.ExecuteScalar( "SELECT DbBase WHERE DbId = @dbid", parms );

				if( item != null ) {
					TypeSwitch.Do( item, TypeSwitch.Case<DbArtist>( artist => SetRating( database, artist, args.Value )),
										 TypeSwitch.Case<DbAlbum>( album => SetRating( database, album, args.Value  )),
										 TypeSwitch.Case<DbTrack>( track => SetRating( database, track, args.Value )),
										 TypeSwitch.Case<DbPlayList>( playList => SetRating( database, playList, args.Value )),
										 TypeSwitch.Default( () => NoiseLogger.Current.LogMessage( String.Format( "Unknown type passed to SetRating: {0}", item.GetType()))));
				}
				else {
					NoiseLogger.Current.LogMessage( String.Format( "Cannot locate item for SetRatingCommand: {0}", args.ItemId ) );
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - SetRating:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}

		private void SetFavorite( IDatabase database, DbArtist forArtist, bool isFavorite ) {
			Condition.Requires( forArtist ).IsNotNull();

			try {
				if(( forArtist != null ) &&
				   ( forArtist.IsFavorite != isFavorite )) {
					forArtist.IsFavorite = isFavorite;
					database.Store( forArtist );

					mEvents.GetEvent<Events.DatabaseItemChanged>().Publish( new DbItemChangedArgs( forArtist, DbItemChanged.Favorite ));
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - SetFavorite(DbArtist):", ex );
			}
		}

		private void SetFavorite( IDatabase database, DbAlbum forAlbum, bool isFavorite ) {
			Condition.Requires( forAlbum ).IsNotNull();

			try {
				if( forAlbum != null ) {
					if( forAlbum.IsFavorite != isFavorite ) {
						forAlbum.IsFavorite = isFavorite;
						database.Store( forAlbum );

						mEvents.GetEvent<Events.DatabaseItemChanged>().Publish( new DbItemChangedArgs( forAlbum, DbItemChanged.Favorite ));
					}

					var parms = database.Database.CreateParameters();
					parms["artistId"] = forAlbum.Artist;

					var artist = database.Database.ExecuteScalar( "SELECT DbArtist WHERE DbId = @artistId", parms ) as DbArtist;

					if( artist != null ) {
						var albumList = database.Database.ExecuteQuery( "SELECT DbAlbum WHERE Artist = @artistId", parms ).OfType<DbAlbum>().ToList();

						artist.HasFavorites = false;

						if( albumList.Any( album => ( album.IsFavorite ) || ( album.HasFavorites ))) {
							artist.HasFavorites = true;
						}

						database.Store( artist );
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - SetFavorite(DbAlbum):", ex );
			}
		}

		private void SetFavorite( IDatabase database, DbTrack forTrack, bool isFavorite ) {
			Condition.Requires( forTrack ).IsNotNull();

			try {
				if( forTrack != null ) {
					if( forTrack.IsFavorite != isFavorite ) {
						forTrack.IsFavorite = isFavorite;
						database.Store( forTrack );

						mEvents.GetEvent<Events.DatabaseItemChanged>().Publish( new DbItemChangedArgs( forTrack, DbItemChanged.Favorite ));
					}

					var parms = database.Database.CreateParameters();
					parms["albumId"] = forTrack.Album;

					var album = database.Database.ExecuteScalar( "SELECT DbAlbum WHERE DbId = @albumId", parms ) as DbAlbum;
					if( album != null ) {
						var trackList = database.Database.ExecuteQuery( "SELECT DbTrack WHERE Album = @albumId", parms ).OfType<DbTrack>().ToList();

						album.HasFavorites = false;

						if( trackList.Any( t => ( t.IsFavorite ))) {
							album.HasFavorites = true;
						}

						database.Store( album );

						parms["artistId"] = album.Artist;

						var artist = database.Database.ExecuteScalar( "SELECT DbArtist WHERE DbId = @artistId", parms ) as DbArtist;
						if( artist != null ) {
							var albumList = database.Database.ExecuteQuery( "SELECT DbAlbum WHERE Artist = @artistId", parms ).OfType<DbAlbum>().ToList();

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
				NoiseLogger.Current.LogException( "Exception - SetFavorite(DbTrack):", ex );
			}
		}

		private void SetFavorite( IDatabase database, DbPlayList forList, bool isFavorite ) {
			Condition.Requires( forList ).IsNotNull();

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
				NoiseLogger.Current.LogException( "Exception - SetFavorite(DbList):", ex );
			}
		}

		private void SetRating( IDatabase database, DbArtist forArtist, Int16 rating ) {
			Condition.Requires( forArtist ).IsNotNull();

			try {
				if(( forArtist != null ) &&
				   ( forArtist.UserRating != rating )) {
					forArtist.UserRating = rating;

					database.Store( forArtist );
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - SetRating(DbArtist):", ex );
			}
		}

		private void SetRating( IDatabase database, DbAlbum forAlbum, Int16 rating ) {
			Condition.Requires( forAlbum ).IsNotNull();

			try {
				if( forAlbum != null ) {
					forAlbum.UserRating = rating;
					database.Store( forAlbum );

					var parms = database.Database.CreateParameters();
					parms["artistId"] = forAlbum.Artist;

					var artist = database.Database.ExecuteScalar( "SELECT DbArtist WHERE DbId = @artistId", parms ) as DbArtist;

					if( artist != null ) {
						var albumList = database.Database.ExecuteQuery( "SELECT DbAlbum WHERE Artist = @artistId", parms ).OfType<DbAlbum>().ToList();
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
				NoiseLogger.Current.LogException( "Exception - SetRating(DbAlbum):", ex );
			}
		}

		private void SetRating( IDatabase database, DbTrack forTrack, Int16 rating ) {
			Condition.Requires( forTrack ).IsNotNull();

			try {
				if( forTrack != null ) {
					forTrack.Rating = rating;
					database.Store( forTrack );

					var parms = database.Database.CreateParameters();
					parms["albumId"] = forTrack.Album;

					var album = database.Database.ExecuteScalar( "SELECT DbAlbum WHERE DbId = @albumId", parms ) as DbAlbum;
					if( album != null ) {
						var trackList = database.Database.ExecuteQuery( "SELECT DbTrack WHERE Album = @albumId", parms ).OfType<DbTrack>().ToList();
						var maxTrackRating = 0;

						foreach( var track in trackList ) {
							if( track.Rating > maxTrackRating ) {
								maxTrackRating = track.Rating;
							}
						}
						album.MaxChildRating = (Int16)maxTrackRating;
						database.Store( album );

						parms["artistId"] = album.Artist;

						var artist = database.Database.ExecuteScalar( "SELECT DbArtist WHERE DbId = @artistId", parms ) as DbArtist;
						if( artist != null ) {
							var albumList = database.Database.ExecuteQuery( "SELECT DbAlbum WHERE Artist = @artistId", parms ).OfType<DbAlbum>().ToList();
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
				NoiseLogger.Current.LogException( "Exception - SetRating(DbTrack):", ex );
			}
		}

		private void SetRating( IDatabase database, DbPlayList forList, Int16 rating ) {
			Condition.Requires( forList ).IsNotNull();

			try {
				if( forList != null ) {
					forList.Rating = rating;

					database.Store( forList );
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - SetRating(DbPlayList):", ex );
			}
		}

		private void OnSetAlbumCover( SetAlbumCoverCommandArgs args ) {
			var database = mDatabaseManager.ReserveDatabase();

			try {
				var parms = database.Database.CreateParameters();

				parms["albumId"] = args.AlbumId;

				var	artworkList = database.Database.ExecuteQuery( "SELECT DbArtwork WHERE Album = @albumId", parms ).OfType<DbArtwork>();

				foreach( var artwork in artworkList ) {
					if(( artwork.IsUserSelection ) &&
					   ( artwork.DbId != args.ArtworkId )) {
						artwork.IsUserSelection = false;

						database.Store( artwork );
					}

					if(( artwork.DbId == args.ArtworkId ) &&
					   (!artwork.IsUserSelection )) {
						artwork.IsUserSelection = true;

						database.Store( artwork );
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - DataUpdates:OnSetAlbumCover", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}
	}
}
