using System;
using System.Threading.Tasks;
using Grpc.Core;
using Noise.Infrastructure.Interfaces;
using Noise.RemoteServer.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteServer.Services {
    class TransportControlService : TransportControl.TransportControlBase {
        private readonly IPlayController		    mPlayController;
        private readonly IPlayQueue				    mPlayQueue;
        private readonly IAudioController           mAudioController;
        private readonly IRemoteServiceFactory      mServiceFactory;
        private readonly INoiseLog                  mLog;

        public TransportControlService( IPlayController playController, IPlayQueue playQueue, IAudioController audioController,
                                        IRemoteServiceFactory serviceFactory, INoiseLog log ) {
            mPlayController = playController;
            mPlayQueue = playQueue;
            mAudioController = audioController;
            mServiceFactory = serviceFactory;
            mLog = log;
        }

        public override Task<TransportCommandResponse> StartPlay( TransportControlEmpty request, ServerCallContext context ) {
            return ExecuteTransportAction( () => { if( mPlayController.CanPlay ) mPlayController.Play(); }, "StartPlay" );
        }

        public override Task<TransportCommandResponse> PausePlay( TransportControlEmpty request, ServerCallContext context ) {
            return ExecuteTransportAction( () => { if( mPlayController.CanPause ) mPlayController.Pause(); }, "PausePlay" );
        }

        public override Task<TransportCommandResponse> StopPlay( TransportControlEmpty request, ServerCallContext context ) {
            return ExecuteTransportAction( () => { if( mPlayController.CanStop ) mPlayController.Stop(); }, "StopPlay" );
        }

        public override Task<TransportCommandResponse> PlayNext( TransportControlEmpty request, ServerCallContext context ) {
            return ExecuteTransportAction( () => { if( mPlayController.CanPlayNextTrack ) mPlayController.PlayNextTrack(); }, "PlayNextTrack" );
        }

        public override Task<TransportCommandResponse> PlayPrevious( TransportControlEmpty request, ServerCallContext context ) {
            return ExecuteTransportAction( () => { if( mPlayController.CanPlayPreviousTrack ) mPlayController.PlayPreviousTrack(); }, "PlayPreviousTrack" );
        }

        public override Task<TransportCommandResponse> ReplayTrack( TransportControlEmpty request, ServerCallContext context ) {
            return ExecuteTransportAction( () => { if( mPlayController.CurrentTrack != null ) mPlayQueue.PlayingTrackReplayCount = 1; }, "ReplayTrack" );
        }

        private Task<TransportCommandResponse> ExecuteTransportAction( Action transportAction, string exceptionTitle ) {
            return Task.Run( () => {
                try {
                    transportAction();
                }
                catch( Exception ex ) {
                    mLog.LogException( exceptionTitle, ex );

                    return new TransportCommandResponse { Success = false, ErrorMessage = ex.Message };
                }

                return new TransportCommandResponse {  Success = true };
            });
        }

        public override async Task StartTransportStatus( TransportControlEmpty request, IServerStreamWriter<TransportInformation> responseStream, ServerCallContext context ) {
            var transportStatusResponder = mServiceFactory.TransportStatusResponder;

            await transportStatusResponder.StartResponder( responseStream, context );
        }

        public override Task<VolumeLevelInformation> GetVolumeLevel( TransportControlEmpty request, ServerCallContext context ) {
            return Task.Run( () => {
                try {
                    return new VolumeLevelInformation{ Success  = true, VolumeLevel = (int)( mAudioController.Volume * 100 ) };
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( GetVolumeLevel ), ex );

                    return new VolumeLevelInformation { Success = false, ErrorMessage = ex.Message };
                }
            });
        }

        public override Task<VolumeLevelInformation> SetVolumeLevel( VolumeLevelInformation request, ServerCallContext context ) {
            return Task.Run( () => {
                try {
                    mAudioController.Volume = Math.Min( 1.0f, Math.Max( 0.0f, request.VolumeLevel / 100.0f ));

                    return new VolumeLevelInformation{ Success  = true, VolumeLevel = (int)( mAudioController.Volume * 100 ) };
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( GetVolumeLevel ), ex );

                    return new VolumeLevelInformation { Success = false, ErrorMessage = ex.Message };
                }
            });
        }
    }
}
