using System.Threading.Tasks;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Services {
    class TransportProvider : BaseProvider<TransportControl.TransportControlClient>, ITransportProvider {
        public TransportProvider( IServiceLocator serviceLocator, IHostInformationProvider hostProvider ) :
            base( serviceLocator, hostProvider  ) {
        }

        public async Task<TransportCommandResponse> Play() {
            if( Client != null ) {
                return await Client.StartPlayAsync( new TransportControlEmpty());
            }

            return default;
        }

        public async Task<TransportCommandResponse> Pause() {
            if( Client != null ) {
                return await Client.PausePlayAsync( new TransportControlEmpty());
            }

            return default;
        }

        public async Task<TransportCommandResponse> Stop() {
            if( Client != null ) {
                return await Client.StopPlayAsync( new TransportControlEmpty());
            }

            return default;
        }

        public async Task<TransportCommandResponse> PlayPrevious() {
            if( Client != null ) {
                return await Client.PlayPreviousAsync( new TransportControlEmpty());
            }

            return default;
        }

        public async Task<TransportCommandResponse> PlayNext() {
            if( Client != null ) {
                return await Client.PlayNextAsync( new TransportControlEmpty());
            }

            return default;
        }

        public async Task<TransportCommandResponse> ReplayTrack() {
            if( Client != null ) {
                return await Client.ReplayTrackAsync( new TransportControlEmpty());
            }

            return default;
        }
    }
}
