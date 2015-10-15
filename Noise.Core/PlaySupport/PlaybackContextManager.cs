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
					mDefaultContext.ReadAllContext( mAudioController );
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

		private void SetContext( PlaybackContext context ) {
			context.WriteContext( mAudioController );

			mEventAggregator.Publish( new Events.AudioParametersChanged());
		}
	}
}
