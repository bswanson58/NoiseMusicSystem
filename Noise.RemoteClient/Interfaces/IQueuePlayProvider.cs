using System;
using System.Threading.Tasks;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Interfaces {
    interface IQueuePlayProvider {
        Task<QueueControlResponse>  QueueTrack( Int64 trackId );

        Task<QueueControlResponse>  Queue( TrackInfo track );
        Task<QueueControlResponse>  Queue( AlbumInfo album );
    }
}
