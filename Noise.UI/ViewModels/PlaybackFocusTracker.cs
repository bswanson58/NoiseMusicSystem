using System;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;

namespace Noise.UI.ViewModels {
	public class PlaybackFocusTracker : IHandle<Events.ArtistFocusRequested>, IHandle<Events.AlbumFocusRequested>, IHandle<Events.PlaybackTrackStarted> {
		private readonly IEventAggregator	mEventAggregator;
		private readonly TimeSpan			mPlayTrackDelay;
		private readonly bool				mEnabled;
		private DateTime					mLastExplorerRequest;

		public PlaybackFocusTracker( IEventAggregator eventAggregator ) {
			mEventAggregator = eventAggregator;

			mPlayTrackDelay = new TimeSpan( 0, 0, 30 );
			mLastExplorerRequest = DateTime.Now - mPlayTrackDelay;

			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );
			if( configuration != null ) {
				mEnabled = configuration.EnablePlaybackLibraryFocus;
			}
		}

		public void Handle( Events.ArtistFocusRequested request ) {
			mLastExplorerRequest = DateTime.Now;
		}

		public void Handle( Events.AlbumFocusRequested request ) {
			mLastExplorerRequest = DateTime.Now;
		}

		public void Handle( Events.PlaybackTrackStarted eventArgs ) {
			if( mEnabled ) {
				if( mLastExplorerRequest + mPlayTrackDelay < DateTime.Now ) {
					var savedTime = mLastExplorerRequest;

					if( eventArgs.Track.Artist != null ) {
						mEventAggregator.Publish( new Events.ArtistFocusRequested( eventArgs.Track.Artist.DbId ));
					}
					if( eventArgs.Track.Album != null ) {
						mEventAggregator.Publish( new Events.AlbumFocusRequested( eventArgs.Track.Album ));
					}

					mLastExplorerRequest = savedTime;
				}
			}
		}
	}
}
