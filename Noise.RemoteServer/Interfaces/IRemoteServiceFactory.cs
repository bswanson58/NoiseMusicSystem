using Noise.RemoteServer.Services;

namespace Noise.RemoteServer.Interfaces {
    interface IRemoteServiceFactory {
        Grpc.Core.Server            HostServer { get; }

        HostStatusResponder         HostStatusResponder { get; }

        ArtistInformationService    ArtistInformationService { get; }
        AlbumInformationService     AlbumInformationService { get; }
        TrackInformationService     TrackInformationService { get; }
    }
}
