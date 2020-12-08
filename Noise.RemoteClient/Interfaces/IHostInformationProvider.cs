using System;
using System.Threading.Tasks;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Interfaces {
    interface IHostInformationProvider {
        Task<HostInformationResponse>   GetHostInformation();

        void                            StopHostStatusRequests();

        IObservable<HostStatusResponse> HostStatus { get; }
    }
}
