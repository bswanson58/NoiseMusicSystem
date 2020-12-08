using Noise.RemoteServer.Services;

namespace Noise.RemoteServer.Interfaces {
    interface IRemoteServiceFactory {
        Grpc.Core.Server            HostServer { get; }

        HostInformationService      HostInformationService { get; }
    }
}
