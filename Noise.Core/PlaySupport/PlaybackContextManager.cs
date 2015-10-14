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
			var album = mAlbumProvider.GetAlbum( forTrack.Album );

			if( album != null ) {
				var newContext = BuildContext( forTrack );

				ChangeContext( mCurrentContext, newContext );

				if( newContext.HasContext()) {
					mCurrentContext = newContext;
					mCurrentTrack = forTrack;
				}
				else {
					mCurrentContext = null;
					mCurrentTrack = null;
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

			retValue.AddContext( mContextWriter.GetAlbumContext( track ));
			retValue.AddContext( mContextWriter.GetTrackContext( track ));

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

			if( context.ReverbValid ) {
				context.PreviousReverbDelay = mAudioController.ReverbDelay;
				context.PreviousReverbLevel = mAudioController.ReverbLevel;
				mAudioController.ReverbDelay = context.ReverbDelay;
				mAudioController.ReverbLevel = context.ReverbLevel;
			}

			if( context.SoftSaturationValid ) {
				context.PreviousSoftSaturationDepth = mAudioController.SoftSaturationDepth;
				context.PreviousSoftSaturationFactor = mAudioController.SoftSaturationFactor;
				mAudioController.SoftSaturationDepth = context.SoftSaturationDepth;
				mAudioController.SoftSaturationFactor = context.SoftSaturationFactor;
			}

			if( context.StereoEnhancerValid ) {
				context.PreviousStereoEnhancerWetDry = mAudioController.StereoEnhancerWetDry;
				context.PreviousStereoEnhancerWidth = mAudioController.StereoEnhancerWidth;
				mAudioController.StereoEnhancerWetDry = context.StereoEnhancerWetDry;
				mAudioController.StereoEnhancerWidth = context.StereoEnhancerWidth;
			}

			if( context.TrackOverlapValid ) {
				context.PreviousTrackOverlap = mAudioController.TrackOverlapMilliseconds;
				mAudioController.TrackOverlapMilliseconds = context.TrackOverlapMilliseconds;
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

			if( context.ReverbValid ) {
				mAudioController.ReverbDelay = context.PreviousReverbDelay;
				mAudioController.ReverbLevel = context.PreviousReverbLevel;
			}

			if( context.SoftSaturationValid ) {
				mAudioController.SoftSaturationDepth = context.PreviousSoftSaturationDepth;
				mAudioController.SoftSaturationFactor = context.PreviousSoftSaturationFactor;
			}

			if( context.StereoEnhancerValid ) {
				mAudioController.StereoEnhancerWetDry = context.PreviousStereoEnhancerWetDry;
				mAudioController.StereoEnhancerWidth = context.PreviousStereoEnhancerWidth;
			}

			if( context.TrackOverlapValid ) {
				mAudioController.TrackOverlapMilliseconds = context.PreviousTrackOverlap;
			}
		}

		private void ChangeContext( PlaybackContext currentContext, PlaybackContext newContext ) {
			if(( currentContext == null ) &&
			   ( newContext != null ) &&
			   ( newContext.HasContext())) {
				SetContext( newContext );
			}

			if(( newContext == null ) &&
			   ( currentContext != null )) {
				ClearContext( currentContext );
			}

			if(( currentContext != null) &&
			   ( newContext != null ) &&
			   ( newContext.HasContext())) {
				var targetContext = new PlaybackContext();

				targetContext.CombineContext( currentContext, newContext );

				SetContext( targetContext );
			}
		}
	}
}
