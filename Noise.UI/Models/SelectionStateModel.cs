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
	public class SelectionStateModel : ISelectionState,
									   IHandle<Events.WindowLayoutRequest>,
									   IHandle<Events.ArtistFocusRequested>, IHandle<Events.AlbumFocusRequested>, IHandle<Events.PlaybackTrackStarted> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IPreferences			mPreferences;
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly Subject<DbArtist>		mArtistSubject;
		private readonly Subject<DbAlbum>		mAlbumSubject;
		private TimeSpan						mPlayTrackDelay;
		private bool							mPlaybackTrackFocusEnabled;
		private DateTime						mLastFocusRequest;

		public	DbArtist						CurrentArtist { get; private set; }
		public	DbAlbum							CurrentAlbum { get; private set; }
		public	IObservable<DbArtist>			CurrentArtistChanged { get { return( mArtistSubject.AsObservable()); }}
		public	IObservable<DbAlbum>			CurrentAlbumChanged { get { return( mAlbumSubject.AsObservable()); }}

		public SelectionStateModel( IEventAggregator eventAggregator, IPreferences preferences, IArtistProvider artistProvider, IAlbumProvider albumProvider ) {
			mEventAggregator = eventAggregator;
			mPreferences = preferences;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;

			mArtistSubject = new Subject<DbArtist>();
			mAlbumSubject = new Subject<DbAlbum>();

			mPlayTrackDelay = new TimeSpan( 0, 0, 30 );
			mLastFocusRequest = DateTime.Now - mPlayTrackDelay;

			var configuration = mPreferences.Load<UserInterfacePreferences>();
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

		public void Handle( Events.ArtistFocusRequested args ) {
			ChangeToArtist( args.ArtistId, true );

			mLastFocusRequest = DateTime.Now;
		}

		public void Handle( Events.AlbumFocusRequested args ) {
			ChangeToArtist( args.ArtistId, true );
			ChangeToAlbum( args.AlbumId );

			mLastFocusRequest = DateTime.Now;
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
		}
	}
}
