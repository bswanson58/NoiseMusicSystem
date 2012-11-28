using System;
using Caliburn.Micro;
using CuttingEdge.Conditions;
using Lpfm.LastFmScrobbler;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using ReusableBits;

namespace Noise.Core.PlayHistory {
	public class PlayScrobbler : IScrobbler, IRequireInitialization {
		private Scrobbler		mScrobbler;
		private bool			mHasNetworkAccess;
		private Track			mNowPlayingTrack;
		private TaskHandler		mScrobblerTaskHandler;

		public PlayScrobbler( ILifecycleManager lifecycleManager ) {
			lifecycleManager.RegisterForInitialize( this );
			lifecycleManager.RegisterForShutdown( this );
		}

		public void TrackStarted( PlayQueueTrack track ) {
			ScrobblePlayingTrack();

			if(( mHasNetworkAccess ) &&
			   ( mScrobbler != null ) &&
			   ( mScrobbler.HasSession ) &&
			   ( track != null ) &&
			   ( track.Track != null )) {
				SetNowPlaying( new Track { ArtistName = track.Artist.Name, TrackName = track.Track.Name, 
										   Duration = track.Track.Duration, WhenStartedPlaying = DateTime.Now });
			}
		}

		public void TrackPlayed( PlayQueueTrack track ) {
			ScrobblePlayingTrack();
		}

		private void ScrobblePlayingTrack() {
			if(( mScrobbler != null ) &&
			   ( mNowPlayingTrack != null )) {
				SetTrackPlayed( mNowPlayingTrack );
				
				mNowPlayingTrack = null;
			}
		}

		internal TaskHandler ScrobblerTaskHander {
			get {
				if( mScrobblerTaskHandler == null ) {
					Execute.OnUIThread( () => mScrobblerTaskHandler = new TaskHandler());
				}

				return( mScrobblerTaskHandler );
			}

			set{ mScrobblerTaskHandler = value; }
		}

		private void SetNowPlaying( Track playingTrack ) {
			ScrobblerTaskHander.StartTask( () => {
					mScrobbler.NowPlaying( playingTrack );
					mNowPlayingTrack = playingTrack;
				},
				() => { },
				exception => NoiseLogger.Current.LogException( "PlayScrobbler:ReportNowPlaying", exception )
			);
		}

		private void SetTrackPlayed( Track playedTrack ) {
			ScrobblerTaskHander.StartTask( () => mScrobbler.Scrobble( playedTrack ),
				() => { },
				exception => NoiseLogger.Current.LogException( "PlayScrobbler:Scrobble", exception )
			);
		}

		public void Initialize() {
			try {
				var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );
				if( configuration != null ) {
					mHasNetworkAccess = configuration.HasNetworkAccess;

					var key = NoiseLicenseManager.Current.RetrieveKey( LicenseKeys.LastFm );

					Condition.Requires( key ).IsNotNull();

					if(( mHasNetworkAccess ) &&
					   ( key != null )) {
						mScrobbler = new Scrobbler( key.Name, key.Key, "011da8bae3b980f1a794fb6eb10b0570" );
						if(!mScrobbler.HasSession ) {
							NoiseLogger.Current.LogInfo( "Scrobbler session could not be created." );
						}
					}
				}

			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Scrobbler:Initialize", ex );
			}
		}

		public void Shutdown() {
			ScrobblePlayingTrack();
		}
	}
}
