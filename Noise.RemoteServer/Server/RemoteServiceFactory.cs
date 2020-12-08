using System;
using Noise.RemoteServer.Interfaces;
using Noise.RemoteServer.Services;

namespace Noise.RemoteServer.Server {
    class RemoteServiceFactory : IRemoteServiceFactory {
        private readonly Func<HostStatusResponder>      mHostStatusResponderCreator;
        private readonly Func<ArtistInformationService> mArtistInformationCreator;

        public RemoteServiceFactory( Func<HostStatusResponder> createHostStatusResponder, Func<ArtistInformationService> artistInformationCreator ) {
            mHostStatusResponderCreator = createHostStatusResponder;
            mArtistInformationCreator = artistInformationCreator;
        }

        public  Grpc.Core.Server            HostServer => new Grpc.Core.Server();

        public  HostStatusResponder         HostStatusResponder => mHostStatusResponderCreator();
        public  ArtistInformationService    ArtistInformationService => mArtistInformationCreator();
    }
}
