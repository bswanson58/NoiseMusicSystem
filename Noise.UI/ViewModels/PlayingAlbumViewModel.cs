using System.Timers;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Logging;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	internal class PlayingAlbumViewModel : AutomaticPropertyBase,
										   IHandle<Events.PlaybackTrackChanged>, IHandle<Events.PlaybackTrackUpdated> {
		private readonly IAlbumArtworkProvider	mArtworkProvider;
		private readonly IPlayQueue				mPlayQueue;
		private readonly IUiLog					mLog;
		private readonly Timer					mTimer;
		private TaskHandler<Artwork>			mArtworkTaskHandler; 
		private DbAlbum							mCurrentAlbum;
		private Artwork							mAlbumCover;
		private int								mImageIndex;

		public PlayingAlbumViewModel( IEventAggregator eventAggregator, IAlbumArtworkProvider artworkProvider, IPlayQueue playQueue, IUiLog log ) {
			mArtworkProvider = artworkProvider;
			mPlayQueue = playQueue;
			mLog = log;
			mTimer = new Timer { AutoReset = true, Interval = 15000, Enabled = false };
			mTimer.Elapsed += OnTimer;

			UpdateAlbum();
			eventAggregator.Subscribe( this );
		}

		public string AlbumName {
			get { return( Get( () => AlbumName )); }
			set {  Set( () => AlbumName, value ); }
		}

		public byte[] AlbumImage {
			get {
				byte[]	retValue = null;

				if( mAlbumCover != null ) {
					retValue = mAlbumCover.Image;
				}

				return( retValue );
			}
		}

		private void ClearAlbum() {
			mCurrentAlbum = null;
			mAlbumCover = null;

			AlbumName = string.Empty;
			StopArtworkTimer();

			RaisePropertyChanged( () => AlbumImage );
		}

		private void SetAlbum( DbAlbum album ) {
			mCurrentAlbum = album;

			if( mCurrentAlbum != null ) {
				AlbumName = mCurrentAlbum.Name;
				mImageIndex = 0;

				RetrieveAlbumArtwork( mCurrentAlbum );

				if( mArtworkProvider.ImageCount( mCurrentAlbum ) > 1 ) {
					StartArtworkTimer();
				}
			}
			else {
				ClearAlbum();
			}
		}

		private void SetAlbumArtwork( Artwork artwork ) {
			mAlbumCover = artwork;
			mImageIndex++;

			RaisePropertyChanged( () => AlbumImage );
		}

		public void Handle( Events.PlaybackTrackChanged args ) {
			UpdateAlbum();
		}

		public void Handle( Events.PlaybackTrackUpdated args ) {
			UpdateAlbum();
		}

		private void UpdateAlbum() {
			if( mPlayQueue.PlayingTrack != null ) {
				if( mCurrentAlbum != null ) {
					if( mPlayQueue.PlayingTrack.Album.DbId != mCurrentAlbum.DbId ) {
						ClearAlbum();
						SetAlbum( mPlayQueue.PlayingTrack.Album );
					}
				}
				else {
					SetAlbum( mPlayQueue.PlayingTrack.Album );
				}
			}
			else {
				ClearAlbum();
			}
		}

		internal TaskHandler<Artwork> ArtworkRetrievalTaskHandler {
			get {
				if( mArtworkTaskHandler == null ) {
					Execute.OnUIThread( () => mArtworkTaskHandler = new TaskHandler<Artwork>());
				}

				return( mArtworkTaskHandler );
			}

			set{ mArtworkTaskHandler = value; }
		}

		private void RetrieveAlbumArtwork( DbAlbum forAlbum ) {
			ArtworkRetrievalTaskHandler.StartTask( 
				() => ( mImageIndex == 0 ? mArtworkProvider.GetAlbumCover( forAlbum ) : mArtworkProvider.GetNextAlbumArtwork( forAlbum, mImageIndex )),
				SetAlbumArtwork,
				exception => mLog.LogException( string.Format( "Retrieving Album Artwork for album:{0}", forAlbum.Name ), exception ) );
		}
		private void StartArtworkTimer() {
			mTimer.Start();
		}

		private void StopArtworkTimer() {
			mTimer.Stop();
		}

		private void OnTimer( object sender, ElapsedEventArgs args ) {
			if( mCurrentAlbum != null ) {
				RetrieveAlbumArtwork( mCurrentAlbum );
			}
		}
	}
}
