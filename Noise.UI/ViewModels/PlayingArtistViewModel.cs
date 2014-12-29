using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class PlayingArtistViewModel : AutomaticPropertyBase,
										 IHandle<Events.PlaybackTrackChanged>, IHandle<Events.PlaybackTrackUpdated> {
		private readonly IMetadataManager	mMetadataManager;
		private readonly IPlayQueue			mPlayQueue;
		private TaskHandler<Artwork>		mArtworkTaskHandler; 
		private DbArtist					mCurrentArtist;
		private Artwork						mArtistImage;

		public PlayingArtistViewModel( IEventAggregator eventAggregator, IMetadataManager metadataManager, IPlayQueue playQueue ) {
			mMetadataManager = metadataManager;
			mPlayQueue = playQueue;

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
		}

		private void SetArtist( DbArtist artist ) {
			mCurrentArtist = artist;

			if( mCurrentArtist != null ) {
				ArtistName = mCurrentArtist.Name;

				RetrieveArtwork( ArtistName );
			}
			else {
				ClearArtist();
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

		private void RetrieveArtwork( string artistName ) {
			ArtworkTaskHandler.StartTask( () => mMetadataManager.GetArtistArtwork( artistName ),
										   SetArtwork,
										   exception => NoiseLogger.Current.LogException( "PlayingArtistViewModel:GetArtistArtwork", exception ));
		}

		private void SetArtwork( Artwork artwork ) {
			mArtistImage = artwork;

			RaisePropertyChanged( () => ArtistImage );
		}
	}
}
