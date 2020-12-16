using System;
using System.Threading.Tasks;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Interfaces {
    interface IQueueListProvider {
        void            StartQueueStatusRequests();
        void            StopQueueStatusRequests();

        Task<bool>      ClearQueue();
        Task<bool>      ClearPlayedTracks();
        Task<bool>      StartStrategyPlay();

        Task<bool>      RemoveQueueItem( QueueTrackInfo track );
        Task<bool>      PromoteQueueItem( QueueTrackInfo track );
        Task<bool>      ReplayQueueItem( QueueTrackInfo track );
        Task<bool>      SkipQueueItem( QueueTrackInfo track );
        Task<bool>      PlayFromQueueItem( QueueTrackInfo track );

        IObservable<QueueStatusResponse>    QueueListStatus { get; }
    }
}
