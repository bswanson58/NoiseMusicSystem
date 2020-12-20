using System;
using System.Threading.Tasks;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Services {
    class TransportProvider : BaseProvider<TransportControl.TransportControlClient>, ITransportProvider {
        private readonly IPlatformLog   mLog;

        public TransportProvider( IServiceLocator serviceLocator, IHostInformationProvider hostProvider, IPlatformLog log ) :
            base( serviceLocator, hostProvider  ) {
            mLog = log;
        }

        public async Task<TransportCommandResponse> Play() {
            if( Client != null ) {
                try {
                    return await Client.StartPlayAsync( new TransportControlEmpty());
                }
                catch( Exception ex ) {
                    mLog.LogException( "Play", ex );
                }
            }

            return default;
        }

        public async Task<TransportCommandResponse> Pause() {
            if( Client != null ) {
                try {
                    return await Client.PausePlayAsync( new TransportControlEmpty());
                }
                catch( Exception ex ) {
                    mLog.LogException( "Pause", ex );
                }
            }

            return default;
        }

        public async Task<TransportCommandResponse> Stop() {
            if( Client != null ) {
                try {
                    return await Client.StopPlayAsync( new TransportControlEmpty());
                }
                catch( Exception ex ) {
                    mLog.LogException( "Stop", ex );
                }
            }

            return default;
        }

        public async Task<TransportCommandResponse> PlayPrevious() {
            if( Client != null ) {
                try {
                    return await Client.PlayPreviousAsync( new TransportControlEmpty());
                }
                catch( Exception ex ) {
                    mLog.LogException( "PlayPrevious", ex );
                }
            }

            return default;
        }

        public async Task<TransportCommandResponse> PlayNext() {
            if( Client != null ) {
                try {
                    return await Client.PlayNextAsync( new TransportControlEmpty());
                }
                catch( Exception ex ) {
                    mLog.LogException( "PlayNext", ex );
                }
            }

            return default;
        }

        public async Task<TransportCommandResponse> ReplayTrack() {
            if( Client != null ) {
                try {
                    return await Client.ReplayTrackAsync( new TransportControlEmpty());
                }
                catch( Exception ex ) {
                    mLog.LogException( "ReplayTrack", ex );
                }
            }

            return default;
        }
    }
}
