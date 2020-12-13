using System;
using System.Threading.Tasks;
using Grpc.Core;
using Noise.Infrastructure.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteServer.Services {
    class TransportControlService : TransportControl.TransportControlBase {
        private readonly IPlayController		    mPlayController;
        private readonly IPlayQueue				    mPlayQueue;
        private readonly INoiseLog                  mLog;

        public TransportControlService( IPlayController playController, IPlayQueue playQueue, INoiseLog log ) {
            mPlayController = playController;
            mPlayQueue = playQueue;
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
    }
}
