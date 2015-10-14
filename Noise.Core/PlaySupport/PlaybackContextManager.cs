using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlaySupport {
	internal class PlaybackContextManager : IPlaybackContextManager {
		private readonly IPlaybackContextWriter	mContextWriter;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly IAudioController		mAudioController;
		private DbTrack							mCurrentTrack;
		private PlaybackContext					mCurrentContext;

		public PlaybackContextManager( IAlbumProvider albumProvider, IAudioController audioController, IPlaybackContextWriter contextWriter ) {
			mAlbumProvider = albumProvider;
			mContextWriter = contextWriter;
			mAudioController = audioController;
		}

		public void OpenContext( DbTrack forTrack ) {
			CloseContext( mCurrentTrack );

			var album = mAlbumProvider.GetAlbum( forTrack.Album );

			if( album != null ) {
				var context = BuildContext( forTrack );

				if( context.HasContext ) {
					SetContext( context );

					mCurrentContext = context;
					mCurrentTrack = forTrack;
				}
			}
		}

		public void CloseContext( DbTrack forTrack ) {
			if(( mCurrentContext != null ) &&
			   ( forTrack != null ) &&
			   ( mCurrentTrack != null ) &&
			   ( mCurrentTrack.DbId == forTrack.DbId )) {
				ClearContext( mCurrentContext );

				mCurrentContext = null;
			}
		}

		private PlaybackContext BuildContext( DbTrack track ) {
			var retValue = new PlaybackContext();

			retValue.CombineContext( mContextWriter.GetAlbumContext( track ));
			retValue.CombineContext( mContextWriter.GetTrackContext( track ));

			return( retValue );
		}

		private void SetContext( PlaybackContext context ) {
			if( context.PanPositionValid ) {
				context.PreviousPanPosition = mAudioController.PanPosition;
				mAudioController.PanPosition = context.PanPosition;
			}

			if( context.PreampVolumeValid ) {
				context.PreviousPreampVolume = mAudioController.PreampVolume;
				mAudioController.PreampVolume = context.PreampVolume;
			}

			if( context.PlaySpeedValid ) {
				context.PreviousPlaySpeed = mAudioController.PlaySpeed;
				mAudioController.PlaySpeed = context.PlaySpeed;
			}
		}

		private void ClearContext( PlaybackContext context ) {
			if( context.PanPositionValid ) {
				mAudioController.PanPosition = context.PreviousPanPosition;
			}

			if( context.PreampVolumeValid ) {
				mAudioController.PreampVolume = context.PreviousPreampVolume;
			}

			if( context.PlaySpeedValid ) {
				mAudioController.PlaySpeed = context.PreviousPlaySpeed;
			}
		}
	}
}
