﻿using System;
using System.Threading.Tasks;
using Noise.RemoteClient.Dto;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Interfaces {
    interface IHostInformationProvider {
        Task<HostInformationResponse>   GetHostInformation();

        void                            StopHostStatusRequests();

        IObservable<HostStatusResponse> HostStatus { get; }
        IObservable<LibraryStatus>      LibraryStatus { get; }
    }
}
