using System.Threading.Tasks;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Services {
    class QueuePlayProvider : BaseProvider<QueueControl.QueueControlClient>, IQueuePlayProvider {
        public QueuePlayProvider( IServiceLocator serviceLocator, IHostInformationProvider hostProvider ) : 
            base( serviceLocator, hostProvider ) { }

        public async Task<QueueControlResponse> QueueTrack( long trackId ) {
            var client = Client;

            if( client != null ) {
                return await client.AddTrackAsync( new AddQueueRequest { ItemId = trackId });
            }

            return default;
        }

        public async Task<QueueControlResponse> Queue( TrackInfo track ) {
            return await QueueTrack( track.TrackId );
        }

        public async Task<QueueControlResponse> Queue( AlbumInfo album ) {
            var client = Client;

            if( client != null ) {
                return await client.AddAlbumAsync( new AddQueueRequest { ItemId = album.AlbumId });
            }

            return default;
        }
    }
}
