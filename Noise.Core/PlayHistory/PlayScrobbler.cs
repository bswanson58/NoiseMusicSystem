using System;
using Caliburn.Micro;
using CuttingEdge.Conditions;
using Lpfm.LastFmScrobbler;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits;

namespace Noise.Core.PlayHistory {
	public class PlayScrobbler : IScrobbler, IRequireInitialization {
		private readonly ILicenseManager	mLicenseManager;
		private readonly IPreferences		mPreferences;
		private Scrobbler					mScrobbler;
		private bool						mEnablePlaybackScrobbling;
		private Track						mNowPlayingTrack;
		private TaskHandler					mScrobblerTaskHandler;

		public PlayScrobbler( ILifecycleManager lifecycleManager, ILicenseManager licenseManager, IPreferences preferences ) {
			mLicenseManager = licenseManager;
			mPreferences = preferences;

			lifecycleManager.RegisterForInitialize( this );
			lifecycleManager.RegisterForShutdown( this );
		}

		public void TrackStarted( PlayQueueTrack track ) {
			ScrobblePlayingTrack();

			if(( mEnablePlaybackScrobbling ) &&
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
			if( playedTrack.Duration > TimeSpan.FromSeconds( 30 )) {
				var timeLimit = playedTrack.WhenStartedPlaying + new TimeSpan( 0, 0, (int)playedTrack.Duration.TotalSeconds / 2 );

				if( DateTime.Now > timeLimit ) {
					ScrobblerTaskHander.StartTask( () => mScrobbler.Scrobble( playedTrack ),
						() => { },
						exception => NoiseLogger.Current.LogException( "PlayScrobbler:Scrobble", exception )
					);
				}
			}
		}

		public void Initialize() {
			try {
				var preferences = mPreferences.Load<NoiseCorePreferences>();

				mEnablePlaybackScrobbling = preferences.HasNetworkAccess && preferences.EnablePlaybackScrobbling;

				var key = mLicenseManager.RetrieveKey( LicenseKeys.LastFm );

				Condition.Requires( key ).IsNotNull();

				if(( mEnablePlaybackScrobbling ) &&
					( key != null )) {
					mScrobbler = new Scrobbler( key.Name, key.Key, "011da8bae3b980f1a794fb6eb10b0570" );
					if(!mScrobbler.HasSession ) {
						NoiseLogger.Current.LogInfo( "Scrobbler session could not be created." );
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
