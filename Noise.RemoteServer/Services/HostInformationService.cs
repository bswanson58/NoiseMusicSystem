using System.Net;
using System.Threading.Tasks;
using Grpc.Core;
using Noise.Infrastructure.RemoteHost;
using Noise.RemoteServer.Interfaces;
using Noise.RemoteServer.Protocol;
using ReusableBits.Platform;

namespace Noise.RemoteServer.Services {
    class HostInformationService : HostInformation.HostInformationBase {
        private readonly IRemoteServiceFactory      mServiceFactory;
        private readonly RemoteHostConfiguration    mHostConfiguration;
        private HostStatusResponder                 mHostStatusResponder;

        public HostInformationService( IRemoteServiceFactory serviceFactory, RemoteHostConfiguration hostConfiguration ) {
            mServiceFactory = serviceFactory;
            mHostConfiguration = hostConfiguration;
        }

        public override Task<HostInformationResponse> GetHostInformation( Empty request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new HostInformationResponse {
                    HostVersion = new Version { Major = VersionInformation.Version.Major, Minor = VersionInformation.Version.Minor,
                                                Build = VersionInformation.Version.Build, Revision = VersionInformation.Version.Revision },
                    ApiVersion = new Version { Major = mHostConfiguration.ApiVersion, Minor = 0, Build = 0, Revision = 0 },
                    HostDescription = mHostConfiguration.ServerName,
                    HostName = Dns.GetHostName(),
                };

                return retValue;
            });
        }

        public override async Task StartHostStatus( Empty _, IServerStreamWriter<HostStatusResponse> responseStream, ServerCallContext context ) {
            StopHostStatusResponder();

            mHostStatusResponder = mServiceFactory.HostStatusResponder;
            await mHostStatusResponder.StartResponder( responseStream, context );
        }

        public void StopHostStatusResponder() {
            mHostStatusResponder?.StopResponder();
            mHostStatusResponder = null;
        }
    }
}
