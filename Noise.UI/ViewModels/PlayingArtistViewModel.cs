using System.Timers;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Logging;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	internal class PlayingArtistViewModel : AutomaticPropertyBase,
											IHandle<Events.PlaybackTrackChanged>, IHandle<Events.PlaybackTrackUpdated> {
		private readonly IArtistArtworkProvider	mArtworkProvider;
		private readonly IPlayQueue				mPlayQueue;
		private readonly IUiLog					mLog;
		private readonly Timer					mTimer;
		private TaskHandler<Artwork>			mArtworkTaskHandler; 
		private DbArtist						mCurrentArtist;
		private Artwork							mArtistImage;

		public PlayingArtistViewModel( IEventAggregator eventAggregator, IArtistArtworkProvider artworkProvider, IPlayQueue playQueue, IUiLog log ) {
			mArtworkProvider = artworkProvider;
			mPlayQueue = playQueue;
			mLog = log;
			mTimer = new Timer { AutoReset = true, Interval = 25000, Enabled = false };
			mTimer.Elapsed += OnTimer;

			UpdateArtist();
			eventAggregator.Subscribe( this );
		}

		public string ArtistName {
			get { return( Get( () => ArtistName )); }
			set {  Set( () => ArtistName, value ); }
		}

		public byte[] ArtistImage {
			get {
				byte[]	retValue = null;

				if( mArtistImage != null ) {
					retValue = mArtistImage.Image;
				}

				return( retValue );
			}
		}

		private void ClearArtist() {
			mCurrentArtist = null;
			mArtistImage = null;

			ArtistName = string.Empty;
			RaisePropertyChanged( () => ArtistImage );

			StopArtworkTimer();
		}

		private void SetArtist( DbArtist artist ) {
			ClearArtist();
			mCurrentArtist = artist;

			if( mCurrentArtist != null ) {
				ArtistName = mCurrentArtist.Name;

				RetrieveArtwork( mCurrentArtist );

				if( mArtworkProvider.ImageCount( mCurrentArtist ) > 1 ) {
					StartArtworkTimer();
				}
			}
		}

		public void Handle( Events.PlaybackTrackChanged args ) {
			UpdateArtist();
		}

		public void Handle( Events.PlaybackTrackUpdated args ) {
			UpdateArtist();
		}

		private void UpdateArtist() {
			if( mPlayQueue.PlayingTrack != null ) {
				if( mCurrentArtist != null ) {
					if( mPlayQueue.PlayingTrack.Artist.DbId != mCurrentArtist.DbId ) {
						ClearArtist();
						SetArtist( mPlayQueue.PlayingTrack.Artist );
					}
				}
				else {
					SetArtist( mPlayQueue.PlayingTrack.Artist );
				}
			}
			else {
				ClearArtist();
			}
		}

		internal TaskHandler<Artwork> ArtworkTaskHandler {
			get {
				if( mArtworkTaskHandler == null ) {
					Execute.OnUIThread( () => mArtworkTaskHandler = new TaskHandler<Artwork>());
				}

				return( mArtworkTaskHandler );
			}

			set { mArtworkTaskHandler = value; }
		}

		private void RetrieveArtwork( DbArtist forArtist ) {
			ArtworkTaskHandler.StartTask( () => mArtworkProvider.GetRandomArtwork( forArtist ),
										   SetArtwork,
										   exception => mLog.LogException( string.Format( "RetrieveArtwork for \"{0}\"", forArtist.Name ), exception ));
		}

		private void SetArtwork( Artwork artwork ) {
			mArtistImage = artwork;

			RaisePropertyChanged( () => ArtistImage );
		}

		private void StartArtworkTimer() {
			mTimer.Start();
		}

		private void StopArtworkTimer() {
			mTimer.Stop();
		}

		private void OnTimer( object sender, ElapsedEventArgs args ) {
			if( mCurrentArtist != null ) {
				RetrieveArtwork( mCurrentArtist );
			}
		}
	}
}
