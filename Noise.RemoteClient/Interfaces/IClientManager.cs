using System;
using Noise.RemoteClient.Dto;

namespace Noise.RemoteClient.Interfaces {
    interface IClientManager {
        void    OnApplicationStarting();
        void    OnApplicationStopping();

        void    StartClientManager();
        void    StopClientManager();

        IObservable<ClientStatus>   ClientStatus { get; }
    }
}
