using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlaySupport {
	internal class PlaybackContextManager : IPlaybackContextManager {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IPlaybackContextWriter	mContextWriter;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly IAudioController		mAudioController;
		private DbTrack							mCurrentTrack;
		private readonly PlaybackContext		mDefaultContext;
		private PlaybackContext					mCurrentContext;

		public PlaybackContextManager( IEventAggregator eventAggregator, IAlbumProvider albumProvider, IAudioController audioController, IPlaybackContextWriter contextWriter ) {
			mEventAggregator = eventAggregator;
			mAlbumProvider = albumProvider;
			mContextWriter = contextWriter;
			mAudioController = audioController;

			mDefaultContext = new PlaybackContext();
		}

		public void OpenContext( DbTrack forTrack ) {
			var album = mAlbumProvider.GetAlbum( forTrack.Album );

			if( album != null ) {
				var newContext = BuildContext( forTrack );

				if( mCurrentContext == null ) {
					CollectDefaultContext( mDefaultContext );
				}

				if( newContext.HasContext()) {
					ChangeContext( mCurrentContext, newContext );

					mCurrentContext = newContext;
					mCurrentTrack = forTrack;
				}
				else {
					ClearContext();
				}
			}
		}

		public void CloseContext( DbTrack forTrack ) {
			if(( mCurrentContext != null ) &&
			   ( forTrack != null ) &&
			   ( mCurrentTrack != null ) &&
			   ( mCurrentTrack.DbId == forTrack.DbId )) {
				ClearContext();
			}
		}

		private PlaybackContext BuildContext( DbTrack track ) {
			var retValue = new PlaybackContext();

			retValue.AddContext( mContextWriter.GetAlbumContext( track ));
			retValue.AddContext( mContextWriter.GetTrackContext( track ));

			return( retValue );
		}

		private void CollectDefaultContext( PlaybackContext context ) {
			context.PanPositionValid = true;
			context.PanPosition = mAudioController.PanPosition;

			context.PlaySpeedValid = true;
			context.PlaySpeed = mAudioController.PlaySpeed;

			context.PreampVolumeValid = true;
			context.PreampVolume = mAudioController.PreampVolume;

			context.ReverbValid = true;
			context.ReverbEnabled = mAudioController.ReverbEnable;
			context.ReverbDelay = mAudioController.ReverbDelay;
			context.ReverbLevel = mAudioController.ReverbLevel;

			context.SoftSaturationValid = true;
			context.SoftSaturationEnabled = mAudioController.SoftSaturationEnable;
			context.SoftSaturationDepth = mAudioController.SoftSaturationDepth;
			context.SoftSaturationFactor = mAudioController.SoftSaturationFactor;

			context.StereoEnhancerValid = true;
			context.StereoEnhancerEnabled = mAudioController.StereoEnhancerEnable;
			context.StereoEnhancerWetDry = mAudioController.StereoEnhancerWetDry;
			context.StereoEnhancerWidth = mAudioController.StereoEnhancerWidth;

			context.TrackOverlapValid = true;
			context.TrackOverlapEnabled = mAudioController.TrackOverlapEnable;
			context.TrackOverlapMilliseconds = mAudioController.TrackOverlapMilliseconds;
		}

		private void SetContext( PlaybackContext context ) {
			if( context.PanPositionValid ) {
				mAudioController.PanPosition = context.PanPosition;
			}

			if( context.PreampVolumeValid ) {
				mAudioController.PreampVolume = context.PreampVolume;
			}

			if( context.PlaySpeedValid ) {
				mAudioController.PlaySpeed = context.PlaySpeed;
			}

			if( context.ReverbValid ) {
				mAudioController.ReverbDelay = context.ReverbDelay;
				mAudioController.ReverbLevel = context.ReverbLevel;
				mAudioController.ReverbEnable = context.ReverbEnabled;
			}

			if( context.SoftSaturationValid ) {
				mAudioController.SoftSaturationDepth = context.SoftSaturationDepth;
				mAudioController.SoftSaturationFactor = context.SoftSaturationFactor;
				mAudioController.SoftSaturationEnable = context.SoftSaturationEnabled;
			}

			if( context.StereoEnhancerValid ) {
				mAudioController.StereoEnhancerWetDry = context.StereoEnhancerWetDry;
				mAudioController.StereoEnhancerWidth = context.StereoEnhancerWidth;
				mAudioController.StereoEnhancerEnable = context.StereoEnhancerEnabled;
			}

			if( context.TrackOverlapValid ) {
				mAudioController.TrackOverlapMilliseconds = context.TrackOverlapMilliseconds;
				mAudioController.TrackOverlapEnable = context.TrackOverlapEnabled;
			}

			mEventAggregator.Publish( new Events.AudioParametersChanged());
		}

		private void ChangeContext( PlaybackContext currentContext, PlaybackContext newContext ) {
			if(( currentContext == null ) &&
			   ( newContext != null ) &&
			   ( newContext.HasContext())) {
				SetContext( newContext );
			}

			if(( currentContext != null) &&
			   ( newContext != null ) &&
			   ( newContext.HasContext())) {
				var targetContext = new PlaybackContext();

				targetContext.CombineContext( mDefaultContext, newContext );

				SetContext( targetContext );
			}

			if(( newContext != null ) &&
			   (!newContext.HasContext())) {
				ClearContext();
			}

			if( newContext == null ) {
				ClearContext();
			}
		}

		private void ClearContext() {
			if( mDefaultContext != null ) {
				SetContext( mDefaultContext );
			}

			mCurrentContext = null;
			mCurrentTrack = null;
		}
	}
}
