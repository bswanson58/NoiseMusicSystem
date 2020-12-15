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

        IObservable<QueueStatusResponse>    QueueListStatus { get; }
    }
}
