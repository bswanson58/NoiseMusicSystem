using Noise.RemoteServer.Services;

namespace Noise.RemoteServer.Interfaces {
    interface IRemoteServiceFactory {
        Grpc.Core.Server            HostServer { get; }

        HostStatusResponder         HostStatusResponder { get; }

        ArtistInformationService    ArtistInformationService { get; }
        AlbumInformationService     AlbumInformationService { get; }
        TrackInformationService     TrackInformationService { get; }
        QueueService                QueueControlService { get; }
        QueueStatusResponder        QueueStatusResponder { get; }
        SearchService               SearchService { get; }
        TagInformationService       TagInformationService { get; }
        TransportControlService     TransportControlService { get; }
        TransportStatusResponder    TransportStatusResponder { get; }
    }
}
