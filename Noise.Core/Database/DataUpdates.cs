using System;
using System.Linq;
using Caliburn.Micro;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.Database {
	internal class DataUpdates : IDataUpdates, IRequireInitialization {
		private readonly IEventAggregator	mEventAggregator;
		private readonly INoiseLog			mLog;
		private readonly IDbBaseProvider	mDbBaseProvider;
		private readonly IArtistProvider	mArtistProvider;
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ITrackProvider		mTrackProvider;
		private readonly IArtworkProvider	mArtworkProvider;
		private readonly IPlayListProvider	mPlayListProvider;

		private AsyncCommand<SetFavoriteCommandArgs>	mSetFavoriteCommand;
		private AsyncCommand<SetRatingCommandArgs>		mSetRatingCommand;
		private AsyncCommand<SetAlbumCoverCommandArgs>	mSetAlbumCoverCommand;

		public DataUpdates( IEventAggregator eventAggregator, ILifecycleManager lifecycleManager, INoiseLog log,
							IDbBaseProvider dbBaseProvider, IPlayListProvider playListProvider,
							IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, IArtworkProvider artworkProvider ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mDbBaseProvider = dbBaseProvider;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mPlayListProvider = playListProvider;
			mArtworkProvider = artworkProvider;

			lifecycleManager.RegisterForInitialize( this );
		}

		public void Initialize() {
			mSetFavoriteCommand = new AsyncCommand<SetFavoriteCommandArgs>( OnSetFavorite );
			mSetFavoriteCommand.ExecutionComplete += OnExecutionComplete;
			GlobalCommands.SetFavorite.RegisterCommand( mSetFavoriteCommand );

			mSetRatingCommand = new AsyncCommand<SetRatingCommandArgs>( OnSetRating );
			mSetRatingCommand.ExecutionComplete += OnExecutionComplete;
			GlobalCommands.SetRating.RegisterCommand( mSetRatingCommand );

			mSetAlbumCoverCommand = new AsyncCommand<SetAlbumCoverCommandArgs>( OnSetAlbumCover );
			mSetAlbumCoverCommand.ExecutionComplete += OnExecutionComplete;
			GlobalCommands.SetAlbumCover.RegisterCommand( mSetAlbumCoverCommand );
		}

		public void Shutdown() {
		}

		private void OnExecutionComplete( object sender, AsyncCommandCompleteEventArgs args ) {
			if(( args != null ) &&
			   ( args.Exception != null )) {
				mLog.LogException( "After command execution", args.Exception );
			}
		}

		private void OnSetFavorite( SetFavoriteCommandArgs args ) {
			Condition.Requires( args ).IsNotNull();

			try {
				var item = mDbBaseProvider.GetItem( args.ItemId );

				if( item != null ) {
					TypeSwitch.Do( item, TypeSwitch.Case<DbArtist>( artist => SetFavorite( artist, args.Value )),
										 TypeSwitch.Case<DbAlbum>( album => SetFavorite( album, args.Value  )),
										 TypeSwitch.Case<DbTrack>( track => SetFavorite( track, args.Value )),
										 TypeSwitch.Case<DbPlayList>( playList => SetFavorite( playList, args.Value )),
										 TypeSwitch.Default( () => mLog.LogMessage( String.Format( "Unknown type passed to SetFavorite: {0}", item.GetType()))));
				}
				else {
					mLog.LogMessage( String.Format( "Cannot locate item for SetFavoriteCommand: {0}", args.ItemId ) );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Setting favorite flag", ex );
			}
		}

		private void OnSetRating( SetRatingCommandArgs args ) {
			Condition.Requires( args ).IsNotNull();

			try {
				var item = mDbBaseProvider.GetItem( args.ItemId );

				if( item != null ) {
					TypeSwitch.Do( item, TypeSwitch.Case<DbArtist>( artist => SetRating( artist, args.Value )),
										 TypeSwitch.Case<DbAlbum>( album => SetRating( album, args.Value  )),
										 TypeSwitch.Case<DbTrack>( track => SetRating( track, args.Value )),
										 TypeSwitch.Case<DbPlayList>( playList => SetRating( playList, args.Value )),
										 TypeSwitch.Default( () => mLog.LogMessage( String.Format( "Unknown type passed to SetRating: {0}", item.GetType()))));
				}
				else {
					mLog.LogMessage( String.Format( "Cannot locate item for SetRatingCommand: {0}", args.ItemId ) );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Setting rating flag", ex );
			}
		}

		private void SetFavorite( DbArtist forArtist, bool isFavorite ) {
			Condition.Requires( forArtist ).IsNotNull();

			try {
				if(( forArtist != null ) &&
				   ( forArtist.IsFavorite != isFavorite )) {
					using( var updater = mArtistProvider.GetArtistForUpdate( forArtist.DbId )) {
						if( updater.Item != null ) {
							updater.Item.IsFavorite = isFavorite;

							updater.Update();
						}
					}

					mEventAggregator.Publish( new Events.ArtistUserUpdate( forArtist.DbId ));
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Setting rating for {0}", forArtist ), ex );
			}
		}

		private void SetFavorite( DbAlbum forAlbum, bool isFavorite ) {
			Condition.Requires( forAlbum ).IsNotNull();

			try {
				if( forAlbum != null ) {
					if( forAlbum.IsFavorite != isFavorite ) {
						using( var updater = mAlbumProvider.GetAlbumForUpdate( forAlbum.DbId )) {
							if( updater.Item != null ) {
								updater.Item.IsFavorite = isFavorite;

								updater.Update();
							}
						}

						mEventAggregator.Publish( new Events.AlbumUserUpdate( forAlbum.DbId ));
					}

					using( var updater = mArtistProvider.GetArtistForUpdate( forAlbum.Artist )) {
						if( updater.Item != null ) {
							updater.Item.HasFavorites = false;

							using( var albumList = mAlbumProvider.GetAlbumList( forAlbum.Artist )) {
								if( albumList.List.Any( album => ( album.IsFavorite ) || ( album.HasFavorites ))) {
									updater.Item.HasFavorites = true;
								}
							}
						
							updater.Update();
						}
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Setting favorite for {0}", forAlbum ), ex );
			}
		}

		private void SetFavorite( DbTrack forTrack, bool isFavorite ) {
			Condition.Requires( forTrack ).IsNotNull();

			try {
				if( forTrack != null ) {
					if( forTrack.IsFavorite != isFavorite ) {
						using( var updater = mTrackProvider.GetTrackForUpdate( forTrack.DbId )) {
							if( updater.Item != null ) {
								updater.Item.IsFavorite = isFavorite;

								updater.Update();
							}
						}

						mEventAggregator.Publish( new Events.TrackUserUpdate( forTrack.DbId ));
					}

					using( var albumUpdater = mAlbumProvider.GetAlbumForUpdate( forTrack.Album )) {
						if( albumUpdater.Item != null ) {
							using( var trackList = mTrackProvider.GetTrackList( forTrack.Album )) {
								albumUpdater.Item.HasFavorites = false;

								if( trackList.List.Any( t => ( t.IsFavorite ))) {
									albumUpdater.Item.HasFavorites = true;
								}

								albumUpdater.Update();
							}


							using( var artistUpdater = mArtistProvider.GetArtistForUpdate( albumUpdater.Item.Artist )) {
								if( artistUpdater.Item != null ) {
									using( var albumList = mAlbumProvider.GetAlbumList( albumUpdater.Item.Artist )) {
										artistUpdater.Item.HasFavorites = false;

										if( albumList.List.Any( a => ( a.IsFavorite ) || ( a.HasFavorites ))) {
											artistUpdater.Item.HasFavorites = true;
										}

										artistUpdater.Update();
									}
								}
							}
						}
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Setting favorite for {0}", forTrack ), ex );
			}
		}

		private void SetFavorite( DbPlayList forList, bool isFavorite ) {
			Condition.Requires( forList ).IsNotNull();

			try {
				if( forList.IsFavorite != isFavorite ) {
					using( var updater = mPlayListProvider.GetPlayListForUpdate( forList.DbId )) {
						if( updater.Item != null ) {
							updater.Item.IsFavorite = isFavorite;

							updater.Update();
						}

						mEventAggregator.Publish( new Events.PlayListUserUpdate( forList.DbId ));
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Setting favorite for {0}", forList ), ex );
			}
		}

		private void SetRating( DbArtist forArtist, Int16 rating ) {
			Condition.Requires( forArtist ).IsNotNull();

			try {
				if(( forArtist != null ) &&
				   ( forArtist.UserRating != rating )) {
					using( var updater = mArtistProvider.GetArtistForUpdate( forArtist.DbId )) {
						if( updater.Item != null ) {
							updater.Item.UserRating = rating;

							updater.Update();
						}

						mEventAggregator.Publish( new Events.ArtistUserUpdate( forArtist.DbId ));
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Setting rating for {0}", forArtist ), ex );
			}
		}

		private void SetRating( DbAlbum forAlbum, Int16 rating ) {
			Condition.Requires( forAlbum ).IsNotNull();

			try {
				if( forAlbum != null ) {
					using( var albumUpdater = mAlbumProvider.GetAlbumForUpdate( forAlbum.DbId )) {
						if( albumUpdater.Item != null ) {
							albumUpdater.Item.UserRating = rating;

							albumUpdater.Update();
						}

						mEventAggregator.Publish( new Events.AlbumUserUpdate( forAlbum.DbId ));
					}

					using( var artistUpdater = mArtistProvider.GetArtistForUpdate( forAlbum.Artist )) {
						if( artistUpdater.Item != null ) {
							using( var albumList = mAlbumProvider.GetAlbumList( forAlbum.Artist )) {
								var maxAlbumRating = 0;

								foreach( var album in albumList.List ) {
									if( album.Rating > maxAlbumRating ) {
										maxAlbumRating = album.Rating;
									}
									if( album.MaxChildRating > maxAlbumRating ) {
										maxAlbumRating = album.MaxChildRating;
									}
								}

								artistUpdater.Item.MaxChildRating = (Int16)maxAlbumRating;

								artistUpdater.Update();
							}

							mEventAggregator.Publish( new Events.ArtistUserUpdate( forAlbum.Artist ));
						}
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Setting rating for {0}", forAlbum ), ex );
			}
		}

		private void SetRating( DbTrack forTrack, Int16 rating ) {
			Condition.Requires( forTrack ).IsNotNull();

			try {
				if( forTrack != null ) {
					using( var trackUpdater = mTrackProvider.GetTrackForUpdate( forTrack.DbId )) {
						if( trackUpdater.Item != null ) {
							trackUpdater.Item.Rating = rating;

							trackUpdater.Update();
						}

						mEventAggregator.Publish( new Events.TrackUserUpdate( forTrack.DbId ));
					}

					using( var albumUpdater = mAlbumProvider.GetAlbumForUpdate( forTrack.Album )) {
						if( albumUpdater.Item != null ) {
							using( var trackList = mTrackProvider.GetTrackList( forTrack.Album )) {
								var maxTrackRating = 0;

								foreach( var track in trackList.List ) {
									if( track.Rating > maxTrackRating ) {
										maxTrackRating = track.Rating;
									}
								}

								albumUpdater.Item.MaxChildRating = (Int16)maxTrackRating;
								albumUpdater.Update();
							}

							mEventAggregator.Publish( new Events.AlbumUserUpdate( forTrack.Album ));

							using( var artistUpdater = mArtistProvider.GetArtistForUpdate( albumUpdater.Item.Artist )) {
								if( artistUpdater.Item != null ) {
									using( var albumList = mAlbumProvider.GetAlbumList( albumUpdater.Item.Artist )) {
										var maxAlbumRating = 0;

										foreach( var a in albumList.List ) {
											if( a.Rating > maxAlbumRating ) {
												maxAlbumRating = a.Rating;
											}
											if( a.MaxChildRating > maxAlbumRating ) {
												maxAlbumRating = a.MaxChildRating;
											}
										}

										artistUpdater.Item.MaxChildRating = (Int16)maxAlbumRating;
										artistUpdater.Update();
									}

									mEventAggregator.Publish( new Events.ArtistUserUpdate( albumUpdater.Item.Artist ));
								}
							}
						}
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Setting rating for {0}", forTrack ), ex );
			}
		}

		private void SetRating( DbPlayList forList, Int16 rating ) {
			Condition.Requires( forList ).IsNotNull();

			try {
				if( forList != null ) {
					using( var updater = mPlayListProvider.GetPlayListForUpdate( forList.DbId )) {
						if( updater.Item != null ) {
							updater.Item.Rating = rating;

							updater.Update();
						}
					}

					mEventAggregator.Publish( new Events.PlayListUserUpdate( forList.DbId ));
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Setting rating for {0}", forList ), ex );
			}
		}

		private void OnSetAlbumCover( SetAlbumCoverCommandArgs args ) {
			try {
				var artworkList = mArtworkProvider.GetAlbumArtwork( args.AlbumId );
				foreach( var artwork in artworkList ) {
					if(( artwork.IsUserSelection ) &&
					   ( artwork.DbId != args.ArtworkId )) {
						using( var artworkUpdater = mArtworkProvider.GetArtworkForUpdate( artwork.DbId )) {
							if( artworkUpdater.Item != null ) {
								artworkUpdater.Item.IsUserSelection = false;

								artworkUpdater.Update();
							}
						}
					}

					if(( artwork.DbId == args.ArtworkId ) &&
					   (!artwork.IsUserSelection )) {
						using( var artworkUpdater = mArtworkProvider.GetArtworkForUpdate( artwork.DbId )) {
							if( artworkUpdater.Item != null ) {
								artworkUpdater.Item.IsUserSelection = true;

								artworkUpdater.Update();
							}
						}
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Setting album cover {0} for Album {1}", args.ArtworkId, args.AlbumId ), ex );
			}
		}
	}
}
