using System;
using Noise.RemoteServer.Interfaces;
using Noise.RemoteServer.Services;

namespace Noise.RemoteServer.Server {
    class RemoteServiceFactory : IRemoteServiceFactory {
        private readonly Func<HostInformationService>   mHostInformationCreator;

        public RemoteServiceFactory( Func<HostInformationService> createHostInformationService ) {
            mHostInformationCreator = createHostInformationService;
        }

        public Grpc.Core.Server         HostServer => new Grpc.Core.Server();
        public HostInformationService   HostInformationService => mHostInformationCreator();
    }
}
