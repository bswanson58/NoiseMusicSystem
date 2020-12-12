using System.Threading.Tasks;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Interfaces {
    interface IQueuePlayProvider {
        Task<QueueControlResponse>  QueueTrack( TrackInfo track );
        Task<QueueControlResponse>  QueueAlbum( AlbumInfo album );
    }
}
