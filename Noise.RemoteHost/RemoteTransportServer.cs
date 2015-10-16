using System;
using System.ServiceModel;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteDto;
using Noise.Infrastructure.RemoteHost;

namespace Noise.RemoteHost {
	[ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]
	public class RemoteTransportServer : INoiseRemoteTransport {
		private readonly IEventAggregator	mEventAggregator;
		private readonly IAudioPlayer		mAudioPlayer;
		private readonly IPlayController	mPlayController;
		private int							mLastChannelStarted;

		public RemoteTransportServer( IEventAggregator eventAggregator, IPlayController playController, IAudioPlayer audioPlayer ) {
			mEventAggregator = eventAggregator;
			mAudioPlayer = audioPlayer;
			mPlayController = playController;

			mAudioPlayer.ChannelStatusChange.Subscribe( OnAudioStatusChanged );
		}

		private void OnAudioStatusChanged( AudioChannelStatus status ) {
			RoTransportState	transportState = null;

			switch( status.Status ) {
				case ePlaybackStatus.TrackStart:
					mLastChannelStarted = status.Channel;
					transportState = BuildTransportState( ePlayState.Playing );
					break;

				case ePlaybackStatus.TrackEnd:
				case ePlaybackStatus.Stopped:
					if( mLastChannelStarted == status.Channel ) {
						transportState = BuildTransportState( ePlayState.Stopped );
					}
					break;
				
				case ePlaybackStatus.Paused:
					transportState = BuildTransportState( ePlayState.Paused );
					break;
			}

			if( transportState != null ) {
				mEventAggregator.Publish( new Events.RemoteTransportUpdate( transportState ));
			}
		}

		public RoTimeSync SyncClientTime( long clientTimeMilliseconds ) {
			return( new RoTimeSync( clientTimeMilliseconds, CurrentMillisecond ));
		}

		public RoTransportState GetTransportState() {
			return( BuildTransportState( mPlayController.PlayState ));
		}

		private RoTransportState BuildTransportState( ePlayState playState ) {
			var retValue = new RoTransportState { PlayState = PlayStateUtility.ConvertPlayState( playState ),
												  ServerTime = CurrentMillisecond,
												  CanPause = mPlayController.CanPause,
												  CanPlay =  mPlayController.CanPlay,
												  CanPlayNextTrack = mPlayController.CanPlayNextTrack,
												  CanPlayPreviousTrack = mPlayController.CanPlayPreviousTrack,
												  CanStop = mPlayController.CanStop,
												  CurrentTrackLength = (long)mAudioPlayer.GetLength( mLastChannelStarted ).TotalMilliseconds,
												  CurrentTrackPosition = (long)mAudioPlayer.GetPlayPosition( mLastChannelStarted ).TotalMilliseconds }; // (long)mPlayController.TrackTime.TotalMilliseconds};

			if( mPlayController.CurrentTrack != null ) {
				retValue.CurrentTrack = mPlayController.CurrentTrack.Uid;
			}

			retValue.Success = true;

			return( retValue );
		}

	    private static readonly DateTime JanuaryFirst1970 = new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );
		private static long CurrentMillisecond {
			get { return((long)(( DateTime.UtcNow - JanuaryFirst1970 ).TotalMilliseconds )); }
		}

		public AudioStateResult GetAudioState() {
			var retValue = new AudioStateResult{ AudioState = { VolumeLevel = (int)( mAudioPlayer.Volume * 100 ) },
												 Success = true };

			return( retValue );
		}

		public BaseResult SetAudioState( RoAudioState audioState ) {
			var retValue = new BaseResult();

			if( audioState.VolumeLevel > 0 ) {
				mAudioPlayer.Volume = (float)audioState.VolumeLevel / 100;
			}

			retValue.Success = true;

			return( retValue );
		}

	}
}
