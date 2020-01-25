using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Interfaces;

namespace Noise.UI.Models {
	internal class SelectionUserState {
		public	long	ArtistId { get; }
		public	long	AlbumId { get; }
		public	long	TagId { get; }

		public SelectionUserState( DbArtist artist, DbAlbum album, DbTag tag ) {
			ArtistId = artist?.DbId ?? Constants.cDatabaseNullOid;
			AlbumId = album?.DbId ?? Constants.cDatabaseNullOid;
			TagId = tag?.DbId ?? Constants.cDatabaseNullOid;
        }
    }

	public class SelectionStateModel : ISelectionState,
									   IHandle<Events.WindowLayoutRequest>, IHandle<Events.DatabaseClosing>, IHandle<Events.LibraryUserState>,
									   IHandle<Events.ArtistFocusRequested>, IHandle<Events.AlbumFocusRequested>, IHandle<Events.TagFocusRequested>,
									   IHandle<Events.TrackQueued>, IHandle<Events.AlbumQueued>,
                                       IHandle<Events.PlaybackTrackStarted>, IHandle<Events.PlaybackStopped> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly ITagProvider			mTagProvider;
		private readonly Subject<DbArtist>		mArtistSubject;
		private readonly Subject<DbAlbum>		mAlbumSubject;
        private readonly Subject<DbTag>         mTagSubject;
        private readonly Subject<string>        mVolumeSubject;
        private readonly Subject<PlayingItem>   mPlayingSubject;
		private TimeSpan						mPlayTrackDelay;
		private bool							mPlaybackTrackFocusEnabled;
		private DateTime						mLastFocusRequest;

		public	DbArtist						CurrentArtist { get; private set; }
		public	DbAlbum							CurrentAlbum { get; private set; }
        public  DbTag                           CurrentTag { get; private set; }
		public	PlayingItem						CurrentlyPlayingItem { get; private set; }

        public	IObservable<DbArtist>			CurrentArtistChanged => ( mArtistSubject.AsObservable());
        public	IObservable<DbAlbum>			CurrentAlbumChanged => ( mAlbumSubject.AsObservable());
        public  IObservable<string>             CurrentAlbumVolumeChanged => mVolumeSubject.AsObservable();
        public  IObservable<DbTag>              CurrentTagChanged => mTagSubject.AsObservable();
        public  IObservable<PlayingItem>        PlayingTrackChanged => mPlayingSubject.AsObservable();

        public SelectionStateModel( IEventAggregator eventAggregator, IPreferences preferences, IArtistProvider artistProvider, IAlbumProvider albumProvider, ITagProvider tagProvider ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTagProvider = tagProvider;

			mArtistSubject = new Subject<DbArtist>();
			mAlbumSubject = new Subject<DbAlbum>();
            mTagSubject = new Subject<DbTag>();
            mVolumeSubject = new Subject<string>();
            mPlayingSubject = new Subject<PlayingItem>();

			mPlayTrackDelay = new TimeSpan( 0, 0, 30 );
			mLastFocusRequest = DateTime.Now - mPlayTrackDelay;

			var configuration = preferences.Load<UserInterfacePreferences>();
			if( configuration != null ) {
				mPlaybackTrackFocusEnabled = configuration.EnablePlaybackLibraryFocus;
			}

			mEventAggregator.Subscribe( this );
		}

		public void SetPlaybackTrackFocus( bool enabled, TimeSpan delay ) {
			mPlaybackTrackFocusEnabled = enabled;
			mPlayTrackDelay = delay;
		}

		public void Handle( Events.WindowLayoutRequest args ) {
			ClearArtist();
			ClearAlbum();
		}

		public void Handle( Events.DatabaseClosing args ) {
			ClearArtist();
			ClearAlbum();

			CurrentTag = new DbTag( eTagGroup.User, String.Empty );
			CurrentlyPlayingItem = new PlayingItem();
        }

		public void Handle( Events.LibraryUserState state ) {
            if( state.IsRestoring ) {
    			if( state.State.ContainsKey( nameof( SelectionStateModel ))) {
	    			var objectState = state.State[nameof( SelectionStateModel )];

		    		if( objectState is SelectionUserState selectionState ) {
                            ChangeToArtist( selectionState.ArtistId, false );
							ChangeToAlbum( selectionState.AlbumId );
							ChangeToTag( selectionState.TagId );
				    }
                }
            }
            else {
                state.State.Add( nameof( SelectionStateModel ), new SelectionUserState( CurrentArtist, CurrentAlbum, CurrentTag ));
            }
        }

		public void Handle( Events.AlbumQueued _ ) {
			mLastFocusRequest = DateTime.Now;
        }

        public void Handle( Events.TrackQueued _ ) {
            mLastFocusRequest = DateTime.Now;
        }

        public void Handle( Events.ArtistFocusRequested args ) {
			ChangeToArtist( args.ArtistId, true );

			mLastFocusRequest = DateTime.Now;
		}

		public void Handle( Events.AlbumFocusRequested args ) {
			ChangeToArtist( args.ArtistId, true );
			ChangeToAlbum( args.AlbumId );

			mLastFocusRequest = DateTime.Now;
		}

        public void Handle( Events.TagFocusRequested args ) {
            ChangeToTag( args.Tag );
        }

		public void Handle( Events.PlaybackTrackStarted args ) {
			if( mPlaybackTrackFocusEnabled ) {
				if( mLastFocusRequest + mPlayTrackDelay < DateTime.Now ) {
					if( args.Track.Artist != null ) {
						ChangeToArtist( args.Track.Artist.DbId, false );
					}
					if( args.Track.Album != null ) {
						ChangeToAlbum( args.Track.Album.DbId );
					}
				}
			}

			CurrentlyPlayingItem = new PlayingItem( args.Track.Artist, args.Track.Album, args.Track.Track );
            mPlayingSubject.OnNext( CurrentlyPlayingItem );
		}

        public void Handle( Events.PlaybackStopped args ) {
			CurrentlyPlayingItem = new PlayingItem();

            mPlayingSubject.OnNext( CurrentlyPlayingItem );
		}

        private void ChangeToArtist( long artistId, bool notifyViewed ) {
			if( artistId == Constants.cDatabaseNullOid ) {
				ClearArtist();
			}
			else {
				if( CurrentArtist != null ) {
					if( CurrentArtist.DbId != artistId ) {
						UpdateArtist( artistId, notifyViewed );
						ClearAlbum();
					}
				}
				else {
					UpdateArtist( artistId, notifyViewed );
				}
			}
		}

		private void ClearArtist() {
			UpdateArtist( null, false );
		}

		private void UpdateArtist( long artistId, bool notifyViewed ) {
			UpdateArtist( mArtistProvider.GetArtist( artistId ), notifyViewed );
		}

		private void UpdateArtist( DbArtist artist, bool notifiedViewed ) {
			CurrentArtist = artist;

			mArtistSubject.OnNext( CurrentArtist );
			mEventAggregator.PublishOnUIThread( new Events.ViewDisplayRequest( ViewNames.ArtistInfoView ));

			if( artist != null ) {
				using( var updater = mArtistProvider.GetArtistForUpdate( artist.DbId )) {
					if( updater.Item != null ) {
						updater.Item.UpdateLastViewed();

						updater.Update();
					}
				}

				if( notifiedViewed ) {
					mEventAggregator.PublishOnUIThread( new Events.ArtistViewed( artist.DbId ) );
				}
			}
		}

		private void ChangeToAlbum( long albumId ) {
			if( albumId == Constants.cDatabaseNullOid ) {
				ClearAlbum();
			}
			else {
				if( CurrentAlbum != null ) {
					if( CurrentAlbum.DbId != albumId ) {
						UpdateAlbum( albumId );
					}
					else {
						mAlbumSubject.OnNext( CurrentAlbum );
					}
				}
				else {
					UpdateAlbum( albumId );
				}
			}
		}

		private void ClearAlbum() {
			UpdateAlbum( null );
		}

		private void UpdateAlbum( long albumId ) {
			UpdateAlbum( mAlbumProvider.GetAlbum( albumId ));
		}

		private void UpdateAlbum( DbAlbum album ) {
			CurrentAlbum = album;
			mAlbumSubject.OnNext( CurrentAlbum );

			if( album != null ) {
				mEventAggregator.PublishOnUIThread( new Events.ViewDisplayRequest( ViewNames.AlbumInfoView ));
			}

            ClearCurrentVolume();
		}

		private void ChangeToTag( long tagId ) {
			ChangeToTag( mTagProvider.GetTag( tagId ));
        }

        private void ChangeToTag( DbTag tag ) {
            CurrentTag = tag;

            mTagSubject.OnNext( CurrentTag );
        }

        public void SetCurrentAlbumVolume( string volumeName ) {
            mVolumeSubject.OnNext( volumeName );
        }

        private void ClearCurrentVolume() {
            SetCurrentAlbumVolume( String.Empty );
        }
	}
}
