using System;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Interfaces {
    interface IQueueListProvider {
        void    StartQueueStatusRequests();
        void    StopQueueStatusRequests();

        IObservable<QueueStatusResponse>    QueueListStatus { get; }
    }
}
