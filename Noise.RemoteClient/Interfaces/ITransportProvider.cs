﻿using System;
using System.Threading.Tasks;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Interfaces {
    interface ITransportProvider {
        Task<TransportCommandResponse> Play();
        Task<TransportCommandResponse> Pause();
        Task<TransportCommandResponse> Stop();
        Task<TransportCommandResponse> PlayNext();
        Task<TransportCommandResponse> PlayPrevious();
        Task<TransportCommandResponse> ReplayTrack();

        void    StartTransportStatusRequests();
        void    StopTransportStatusRequests();

        IObservable<TransportInformation>   TransportStatus { get; }
    }
}
