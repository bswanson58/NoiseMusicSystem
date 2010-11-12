using System;
using System.Linq;
using CuttingEdge.Conditions;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.Database {
	internal class DataUpdates : IDataUpdates {
		private readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEvents;
		private readonly IDatabaseManager	mDatabaseManager;
		private readonly ILog				mLog;

		private AsyncCommand<SetFavoriteCommandArgs>	mSetFavoriteCommand;
		private AsyncCommand<SetRatingCommandArgs>		mSetRatingCommand;

		public DataUpdates( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();
			mDatabaseManager = mContainer.Resolve<IDatabaseManager>();
			mLog = mContainer.Resolve<ILog>();
		}

		public bool Initialize() {
			mSetFavoriteCommand = new AsyncCommand<SetFavoriteCommandArgs>( OnSetFavorite );
			mSetFavoriteCommand.ExecutionComplete += OnExecutionComplete;
			GlobalCommands.SetFavorite.RegisterCommand( mSetFavoriteCommand );

			mSetRatingCommand = new AsyncCommand<SetRatingCommandArgs>( OnSetRating );
			GlobalCommands.SetRating.RegisterCommand( mSetRatingCommand );

			return( true );
		}

		private void OnExecutionComplete( object sender, AsyncCommandCompleteEventArgs args ) {
			if(( args != null ) &&
			   ( args.Exception != null )) {
				mLog.LogException( "Exception - DataUpdates:OnExecutionComplete:", args.Exception );
			}
		}

		private void OnSetFavorite( SetFavoriteCommandArgs args ) {
			Condition.Requires( args ).IsNotNull();

			var database = mDatabaseManager.ReserveDatabase();

			try {
				var item = ( from DbBase o in database.Database where o.DbId == args.ItemId select o ).FirstOrDefault();

				if( item != null ) {
					TypeSwitch.Do( item, TypeSwitch.Case<DbArtist>( artist => SetFavorite( database, artist, args.Value )),
										 TypeSwitch.Case<DbAlbum>( album => SetFavorite( database, album, args.Value  )),
										 TypeSwitch.Case<DbTrack>( track => SetFavorite( database, track, args.Value )),
										 TypeSwitch.Case<DbPlayList>( playList => SetFavorite( database, playList, args.Value )),
										 TypeSwitch.Default( () => mLog.LogMessage( String.Format( "Unknown type passed to SetFavorite: {0}", item.GetType()))));
				}
				else {
					mLog.LogMessage( String.Format( "Cannot locate item for SetFavoriteCommand: {0}", args.ItemId ) );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - SetFavorite:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}

		private void OnSetRating( SetRatingCommandArgs args ) {
			Condition.Requires( args ).IsNotNull();

			var database = mDatabaseManager.ReserveDatabase();

			try {
				var item = ( from DbBase o in database.Database where o.DbId == args.ItemId select o ).FirstOrDefault();

				if( item != null ) {
					TypeSwitch.Do( item, TypeSwitch.Case<DbArtist>( artist => SetRating( database, artist, args.Value )),
										 TypeSwitch.Case<DbAlbum>( album => SetRating( database, album, args.Value  )),
										 TypeSwitch.Case<DbTrack>( track => SetRating( database, track, args.Value )),
										 TypeSwitch.Case<DbPlayList>( playList => SetRating( database, playList, args.Value )),
										 TypeSwitch.Default( () => mLog.LogMessage( String.Format( "Unknown type passed to SetRating: {0}", item.GetType()))));
				}
				else {
					mLog.LogMessage( String.Format( "Cannot locate item for SetRatingCommand: {0}", args.ItemId ) );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - SetRating:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}

		private void SetFavorite( IDatabase database, DbArtist forArtist, bool isFavorite ) {
			Condition.Requires( forArtist ).IsNotNull();

			try {
				forArtist = database.ValidateOnThread( forArtist ) as DbArtist;
				if(( forArtist != null ) &&
				   ( forArtist.IsFavorite != isFavorite )) {
					forArtist.IsFavorite = isFavorite;
					database.Store( forArtist );

					mEvents.GetEvent<Events.DatabaseItemChanged>().Publish( new DbItemChangedArgs( forArtist, DbItemChanged.Favorite ));
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - SetFavorite(DbArtist):", ex );
			}
		}

		private void SetFavorite( IDatabase database, DbAlbum forAlbum, bool isFavorite ) {
			Condition.Requires( forAlbum ).IsNotNull();

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
		}

		private void SetFavorite( IDatabase database, DbTrack forTrack, bool isFavorite ) {
			Condition.Requires( forTrack ).IsNotNull();

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
				mLog.LogException( "Exception - SetFavorite(DbList):", ex );
			}
		}

		private void SetRating( IDatabase database, DbArtist forArtist, Int16 rating ) {
			Condition.Requires( forArtist ).IsNotNull();

			try {
				forArtist = database.ValidateOnThread( forArtist ) as DbArtist;
				if(( forArtist != null ) &&
				   ( forArtist.UserRating != rating )) {
					forArtist.UserRating = rating;

					database.Store( forArtist );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - SetRating(DbArtist):", ex );
			}
		}

		private void SetRating( IDatabase database, DbAlbum forAlbum, Int16 rating ) {
			Condition.Requires( forAlbum ).IsNotNull();

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
		}

		private void SetRating( IDatabase database, DbTrack forTrack, Int16 rating ) {
			Condition.Requires( forTrack ).IsNotNull();

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
		}

		private void SetRating( IDatabase database, DbPlayList forList, Int16 rating ) {
			Condition.Requires( forList ).IsNotNull();

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
		}
	}
}
