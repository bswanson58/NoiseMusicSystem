using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.UI.Models {
	public interface ISelectionState {
		DbArtist				CurrentArtist { get; }
		DbAlbum					CurrentAlbum { get; }

		IObservable<DbArtist>	CurrentArtistChanged { get; }
		IObservable<DbAlbum>	CurrentAlbumChanged { get; } 
	}

	public class SelectionStateModel : ISelectionState,
									   IHandle<Events.ArtistFocusRequested>, IHandle<Events.AlbumFocusRequested>, IHandle<Events.PlaybackTrackStarted> {
		private readonly IEventAggregator		mEventAggregator;
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

		public SelectionStateModel( IEventAggregator eventAggregator, IArtistProvider artistProvider, IAlbumProvider albumProvider ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;

			mArtistSubject = new Subject<DbArtist>();
			mAlbumSubject = new Subject<DbAlbum>();

			mPlayTrackDelay = new TimeSpan( 0, 0, 30 );
			mLastFocusRequest = DateTime.Now - mPlayTrackDelay;

			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );
			if( configuration != null ) {
				mPlaybackTrackFocusEnabled = configuration.EnablePlaybackLibraryFocus;
			}

			mEventAggregator.Subscribe( this );
		}

		public void SetPlaybackTrackFocus( bool enabled, TimeSpan delay ) {
			mPlaybackTrackFocusEnabled = enabled;
			mPlayTrackDelay = delay;
		}

		public void Handle( Events.ArtistFocusRequested args ) {
			ChangeToArtist( args.ArtistId );

			mLastFocusRequest = DateTime.Now;
		}

		public void Handle( Events.AlbumFocusRequested args ) {
			ChangeToArtist( args.ArtistId );
			ChangeToAlbum( args.AlbumId );

			mLastFocusRequest = DateTime.Now;
		}

		public void Handle( Events.PlaybackTrackStarted args ) {
			if( mPlaybackTrackFocusEnabled ) {
				if( mLastFocusRequest + mPlayTrackDelay < DateTime.Now ) {
					if( args.Track.Artist != null ) {
						ChangeToArtist( args.Track.Artist.DbId );
					}
					if( args.Track.Album != null ) {
						ChangeToAlbum( args.Track.Album.DbId );
					}
				}
			}
		}

		private void ChangeToArtist( long artistId ) {
			if( artistId == Constants.cDatabaseNullOid ) {
				ClearArtist();
			}
			else {
				if( CurrentArtist != null ) {
					if( CurrentArtist.DbId != artistId ) {
						UpdateArtist( artistId );
						ClearAlbum();
					}
				}
				else {
					UpdateArtist( artistId );
				}
			}
		}

		private void ClearArtist() {
			UpdateArtist( null );
		}

		private void UpdateArtist( long artistId ) {
			UpdateArtist( mArtistProvider.GetArtist( artistId ));
		}

		private void UpdateArtist( DbArtist artist ) {
			CurrentArtist = artist;
			mArtistSubject.OnNext( CurrentArtist );
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
		}
	}
}
