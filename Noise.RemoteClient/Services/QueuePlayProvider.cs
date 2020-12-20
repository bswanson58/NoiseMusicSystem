using System;
using System.Threading.Tasks;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Services {
    class QueuePlayProvider : BaseProvider<QueueControl.QueueControlClient>, IQueuePlayProvider {
        private readonly IPlatformLog   mLog;

        public QueuePlayProvider( IServiceLocator serviceLocator, IHostInformationProvider hostProvider, IPlatformLog log ) : 
            base( serviceLocator, hostProvider ) {
            mLog = log;
        }

        public async Task<QueueControlResponse> QueueTrack( long trackId ) {
            var client = Client;

            if( client != null ) {
                try {
                    return await client.AddTrackAsync( new AddQueueRequest { ItemId = trackId });
                }
                catch( Exception ex ) {
                    mLog.LogException( "QueueTrack", ex );
                }
            }

            return default;
        }

        public async Task<QueueControlResponse> Queue( TrackInfo track ) {
            return await QueueTrack( track.TrackId );
        }

        public async Task<QueueControlResponse> Queue( AlbumInfo album ) {
            var client = Client;

            if( client != null ) {
                try {
                    return await client.AddAlbumAsync( new AddQueueRequest { ItemId = album.AlbumId });
                }
                catch( Exception ex ) {
                    mLog.LogException( "QueueAlbum", ex );
                }
            }

            return default;
        }
    }
}
