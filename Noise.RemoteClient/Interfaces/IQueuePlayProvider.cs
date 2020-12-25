using System;
using System.Threading.Tasks;
using Noise.RemoteClient.Dto;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Interfaces {
    interface IQueuePlayProvider {
        Task<QueueControlResponse>  Queue( TrackInfo track, bool playNext );
        Task<QueueControlResponse>  Queue( AlbumInfo album );

        IObservable<QueuedItem>     ItemQueued { get; }
    }
}
