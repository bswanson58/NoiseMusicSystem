using System;
using System.Linq;
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
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly IPlayQueue			mPlayQueue;
		private readonly IUiLog				mLog;
		private TaskHandler<Artwork>		mArtworkTaskHandler; 
		private DbAlbum						mCurrentAlbum;
		private Artwork						mAlbumCover;

		public PlayingAlbumViewModel( IEventAggregator eventAggregator, IAlbumProvider albumProvider, IPlayQueue playQueue, IUiLog log ) {
			mAlbumProvider = albumProvider;
			mPlayQueue = playQueue;
			mLog = log;

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
			RaisePropertyChanged( () => AlbumImage );
		}

		private void SetAlbum( DbAlbum album ) {
			mCurrentAlbum = album;

			if( mCurrentAlbum != null ) {
				AlbumName = mCurrentAlbum.Name;

				RetrieveAlbumArtwork( mCurrentAlbum.DbId );
			}
			else {
				ClearAlbum();
			}
		}

		private void SetAlbumArtwork( Artwork artwork ) {
			mAlbumCover = artwork;

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

		private void RetrieveAlbumArtwork(long albumId  ) {
			ArtworkRetrievalTaskHandler.StartTask( () => {
				var albumInfo = mAlbumProvider.GetAlbumSupportInfo( albumId );
				Artwork	cover = null;

				if(( albumInfo.AlbumCovers != null ) &&
				   ( albumInfo.AlbumCovers.GetLength( 0 ) > 0 )) {
					cover = (( from Artwork artwork in albumInfo.AlbumCovers where artwork.IsUserSelection select artwork ).FirstOrDefault() ??
							 ( from Artwork artwork in albumInfo.AlbumCovers where artwork.Source == InfoSource.File select artwork ).FirstOrDefault() ??
							 ( from Artwork artwork in albumInfo.AlbumCovers where artwork.Source == InfoSource.Tag select artwork ).FirstOrDefault()) ??
								albumInfo.AlbumCovers[0];
				}

				if(( cover == null ) &&
				   ( albumInfo.Artwork != null ) &&
				   ( albumInfo.Artwork.GetLength( 0 ) > 0 )) {
					cover = ( from Artwork artwork in albumInfo.Artwork
							  where artwork.Name.IndexOf( "front", StringComparison.InvariantCultureIgnoreCase ) >= 0 select artwork ).FirstOrDefault();

					if(( cover == null ) &&
					   ( albumInfo.Artwork.GetLength( 0 ) == 1 )) {
						cover = albumInfo.Artwork[0];
					}
				}

				return( cover );
			},
			SetAlbumArtwork,
			exception => mLog.LogException( string.Format( "Retrieving Album Artwork for Album:{0}", albumId ), exception ) );
		}

	}
}
