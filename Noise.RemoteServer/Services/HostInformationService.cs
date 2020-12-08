using System;
using System.Net;
using System.Threading.Tasks;
using Grpc.Core;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteHost;
using Noise.RemoteServer.Protocol;
using ReusableBits.Platform;

namespace Noise.RemoteServer.Services {
    class HostInformationService : HostInformation.HostInformationBase {
        private readonly ILibraryConfiguration      mLibraryConfiguration;
        private readonly RemoteHostConfiguration    mHostConfiguration;

        public HostInformationService( ILibraryConfiguration libraryConfiguration, RemoteHostConfiguration hostConfiguration ) {
            mLibraryConfiguration = libraryConfiguration;
            mHostConfiguration = hostConfiguration;
        }

        public override Task<HostInformationResponse> GetHostInformation( Empty request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new HostInformationResponse {
                    HostVersion = new Protocol.Version { Major = VersionInformation.Version.Major, Minor = VersionInformation.Version.Minor,
                                                         Build = VersionInformation.Version.Build, Revision = VersionInformation.Version.Revision },
                    ApiVersion = new Protocol.Version { Major = mHostConfiguration.ApiVersion, Minor = 0, Build = 0, Revision = 0 },
                    HostDescription = mHostConfiguration.ServerName,
                    HostName = Dns.GetHostName(),
                    LibraryName = mLibraryConfiguration.Current != null ? mLibraryConfiguration.Current.LibraryName : String.Empty
                };

                return retValue;
            });
        }
    }
}
