using System;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits;

namespace Noise.Core.DataProviders {
	internal class RatingsUpdater : IRatings {
		private readonly IEventAggregator	mEventAggregator;
		private readonly INoiseLog			mLog;
		private readonly IArtistProvider	mArtistProvider;
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ITrackProvider		mTrackProvider;
		private readonly ISidecarUpdater	mSidecarUpdater;
		private TaskHandler					mUpdateTask;

		public RatingsUpdater( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, ISidecarUpdater sidecarUpdater,
							   IEventAggregator eventAggregator, INoiseLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mSidecarUpdater = sidecarUpdater;
		}

		internal TaskHandler UpdateTask {
			get {
				if( mUpdateTask == null ) {
					Execute.OnUIThread( () => mUpdateTask = new TaskHandler() );
				}

				return( mUpdateTask );
			}
			set {  mUpdateTask = value; }
		}

		public void SetRating( DbArtist forArtist, short rating ) {
			UpdateTask.StartTask( () => UpdateRating( forArtist, rating ),
								  () => {},
								  error => mLog.LogException( "Error running task to update artist rating.", error ));
		}

		private void UpdateRating( DbArtist forArtist, short rating ) {
			try {
				if(( forArtist != null ) &&
				   ( forArtist.UserRating != rating )) {
					using( var updater = mArtistProvider.GetArtistForUpdate( forArtist.DbId )) {
						if( updater.Item != null ) {
							updater.Item.UserRating = rating;

							updater.Update();
						}
					}

					mSidecarUpdater.UpdateSidecar( mArtistProvider.GetArtist( forArtist.DbId ));
					mEventAggregator.Publish( new Events.ArtistUserUpdate( forArtist.DbId ));
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Setting rating for {0}", forArtist ), ex );
			}
		}

		public void SetRating( DbAlbum forAlbum, short rating ) {
			UpdateTask.StartTask( () => UpdateRating( forAlbum, rating ),
								  () => {},
								  error => mLog.LogException( "Error running task to update album rating.", error ));
		}

		private void UpdateRating( DbAlbum forAlbum, short rating ) {
			try {
				if(( forAlbum != null ) &&
				   ( forAlbum.Rating != rating )) {
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
								artistUpdater.Item.CalculatedRating = (Int16)( albumList.List.Sum( a => a.Rating ) / albumList.List.Count());
								artistUpdater.Update();
							}

							mEventAggregator.Publish( new Events.ArtistUserUpdate( forAlbum.Artist ));
						}
					}

					mSidecarUpdater.UpdateSidecar( mAlbumProvider.GetAlbum( forAlbum.DbId ));
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Setting rating for {0}", forAlbum ), ex );
			}
		}

		public void SetRating( DbTrack forTrack, short rating ) {
			UpdateTask.StartTask( () => UpdateRating( forTrack, rating ),
								  () => {},
								  error => mLog.LogException( "Error running task to update track rating.", error ));
		}

		private void UpdateRating( DbTrack forTrack, short rating ) {
			try {
				if(( forTrack != null ) &&
				   ( forTrack.Rating != rating )) {
					using( var trackUpdater = mTrackProvider.GetTrackForUpdate( forTrack.DbId )) {
						if( trackUpdater.Item != null ) {
							trackUpdater.Item.Rating = rating;

							trackUpdater.Update();
						}
					}

					using( var albumUpdater = mAlbumProvider.GetAlbumForUpdate( forTrack.Album )) {
						if( albumUpdater.Item != null ) {
							using( var trackList = mTrackProvider.GetTrackList( forTrack.Album )) {
								albumUpdater.Item.CalculatedRating = (Int16)( trackList.List.Sum( t => t.Rating ) / trackList.List.Count());
								albumUpdater.Update();
							}

							mEventAggregator.Publish( new Events.AlbumUserUpdate( forTrack.Album ));

							using( var artistUpdater = mArtistProvider.GetArtistForUpdate( albumUpdater.Item.Artist )) {
								if( artistUpdater.Item != null ) {
									using( var albumList = mAlbumProvider.GetAlbumList( albumUpdater.Item.Artist )) {
										artistUpdater.Item.CalculatedRating = (Int16)( albumList.List.Sum( a => a.Rating ) / albumList.List.Count());
										artistUpdater.Update();
									}

									mEventAggregator.Publish( new Events.ArtistUserUpdate( albumUpdater.Item.Artist ));
								}
							}
						}
					}

					var track = mTrackProvider.GetTrack( forTrack.DateAddedTicks );

					mSidecarUpdater.UpdateSidecar( track );
					mEventAggregator.Publish( new Events.TrackUserUpdate( track ));
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Setting rating for {0}", forTrack ), ex );
			}
		}

		public void SetFavorite( DbArtist forArtist, bool isFavorite ) {
			UpdateTask.StartTask( () => UpdateFavorite( forArtist, isFavorite ),
								  () => {},
								  error => mLog.LogException( "Error running task to update artist IsFavorite.", error ));
		}

		private void UpdateFavorite( DbArtist forArtist, bool isFavorite ) {
			try {
				if(( forArtist != null ) &&
				   ( forArtist.IsFavorite != isFavorite )) {
					using( var updater = mArtistProvider.GetArtistForUpdate( forArtist.DbId )) {
						if( updater.Item != null ) {
							updater.Item.IsFavorite = isFavorite;

							updater.Update();
						}
					}

					mSidecarUpdater.UpdateSidecar( mArtistProvider.GetArtist( forArtist.DbId ));
					mEventAggregator.Publish( new Events.ArtistUserUpdate( forArtist.DbId ));
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Setting rating for {0}", forArtist ), ex );
			}
		}

		public void SetFavorite( DbAlbum forAlbum, bool isFavorite ) {
			UpdateTask.StartTask( () => UpdateFavorite( forAlbum, isFavorite ),
								  () => {},
								  error => mLog.LogException( "Error running task to update album IsFavorite.", error ));
		}

		private void UpdateFavorite( DbAlbum forAlbum, bool isFavorite ) {
			try {
				if(( forAlbum != null ) &&
				   ( forAlbum.IsFavorite != isFavorite )) {
					using( var updater = mAlbumProvider.GetAlbumForUpdate( forAlbum.DbId )) {
						if( updater.Item != null ) {
							updater.Item.IsFavorite = isFavorite;

							updater.Update();
						}
					}

					using( var updater = mArtistProvider.GetArtistForUpdate( forAlbum.Artist )) {
						if( updater.Item != null ) {
							using( var albumList = mAlbumProvider.GetAlbumList( forAlbum.Artist )) {
								updater.Item.HasFavorites = albumList.List.Any( album => ( album.IsFavorite ) || ( album.HasFavorites ));
		
								updater.Update();
							}

							mEventAggregator.Publish( new Events.ArtistUserUpdate( forAlbum.Artist ));
						}
					}

					mSidecarUpdater.UpdateSidecar( mAlbumProvider.GetAlbum( forAlbum.DbId ));
					mEventAggregator.Publish( new Events.AlbumUserUpdate( forAlbum.DbId ));
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Setting favorite for {0}", forAlbum ), ex );
			}
		}

		public void SetFavorite( DbTrack forTrack, bool isFavorite ) {
			UpdateTask.StartTask( () => UpdateFavorite( forTrack, isFavorite ),
								  () => {},
								  error => mLog.LogException( "Error running task to update track IsFavorite.", error ));
		}

		private void UpdateFavorite( DbTrack forTrack, bool isFavorite ) {
			try {
				if(( forTrack != null ) &&
				   ( forTrack.IsFavorite != isFavorite )) {
					using( var updater = mTrackProvider.GetTrackForUpdate( forTrack.DbId )) {
						if( updater.Item != null ) {
							updater.Item.IsFavorite = isFavorite;

							updater.Update();
						}
					}

					using( var albumUpdater = mAlbumProvider.GetAlbumForUpdate( forTrack.Album )) {
						if( albumUpdater.Item != null ) {
							using( var trackList = mTrackProvider.GetTrackList( forTrack.Album )) {
								albumUpdater.Item.HasFavorites = trackList.List.Any( t => ( t.IsFavorite ));

								albumUpdater.Update();
							}

							mEventAggregator.Publish( new Events.AlbumUserUpdate( forTrack.Album ));

							using( var artistUpdater = mArtistProvider.GetArtistForUpdate( albumUpdater.Item.Artist )) {
								if( artistUpdater.Item != null ) {
									using( var albumList = mAlbumProvider.GetAlbumList( albumUpdater.Item.Artist )) {
										artistUpdater.Item.HasFavorites = albumList.List.Any( a => ( a.IsFavorite ) || ( a.HasFavorites ));

										artistUpdater.Update();
									}

									mEventAggregator.Publish( new Events.ArtistUserUpdate( albumUpdater.Item.Artist ));
								}
							}
						}
					}

					var track = mTrackProvider.GetTrack( forTrack.DbId );

					mSidecarUpdater.UpdateSidecar( track );
					mEventAggregator.Publish( new Events.TrackUserUpdate( track ));
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Setting favorite for {0}", forTrack ), ex );
			}
		}
	}
}
