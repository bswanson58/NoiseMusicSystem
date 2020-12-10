using System;
using Noise.RemoteServer.Interfaces;
using Noise.RemoteServer.Services;

namespace Noise.RemoteServer.Server {
    class RemoteServiceFactory : IRemoteServiceFactory {
        private readonly Func<HostStatusResponder>      mHostStatusResponderCreator;
        private readonly Func<ArtistInformationService> mArtistInformationCreator;
        private readonly Func<AlbumInformationService>  mAlbumInformationCreator;
        private readonly Func<TrackInformationService>  mTrackInformationCreator;

        public RemoteServiceFactory( Func<HostStatusResponder> createHostStatusResponder, Func<ArtistInformationService> artistInformationCreator,
                                     Func<AlbumInformationService> albumInformationCreator, Func<TrackInformationService> trackinformationCreator ) {
            mHostStatusResponderCreator = createHostStatusResponder;
            mAlbumInformationCreator = albumInformationCreator;
            mArtistInformationCreator = artistInformationCreator;
            mTrackInformationCreator = trackinformationCreator;
        }

        public  Grpc.Core.Server            HostServer => new Grpc.Core.Server();

        public  HostStatusResponder         HostStatusResponder => mHostStatusResponderCreator();
        public  ArtistInformationService    ArtistInformationService => mArtistInformationCreator();
        public  AlbumInformationService     AlbumInformationService => mAlbumInformationCreator();
        public  TrackInformationService     TrackInformationService => mTrackInformationCreator();
    }
}
