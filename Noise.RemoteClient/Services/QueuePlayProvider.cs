using System.Threading.Tasks;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Services {
    class QueuePlayProvider : BaseProvider<QueueControl.QueueControlClient>, IQueuePlayProvider {
        public QueuePlayProvider( IServiceLocator serviceLocator, IHostInformationProvider hostProvider ) : 
            base( serviceLocator, hostProvider ) { }

        public async Task<QueueControlResponse> QueueTrack( TrackInfo track ) {
            var client = Client;

            if( client != null ) {
                return await client.AddTrackAsync( new AddQueueRequest { ItemId = track.TrackId });
            }

            return default;
        }

        public async Task<QueueControlResponse> QueueAlbum( AlbumInfo album ) {
            var client = Client;

            if( client != null ) {
                return await client.AddAlbumAsync( new AddQueueRequest { ItemId = album.AlbumId });
            }

            return default;
        }
    }
}
