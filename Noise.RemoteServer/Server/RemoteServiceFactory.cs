﻿using System;
using Noise.RemoteServer.Interfaces;
using Noise.RemoteServer.Services;

namespace Noise.RemoteServer.Server {
    class RemoteServiceFactory : IRemoteServiceFactory {
        private readonly Func<HostStatusResponder>      mHostStatusResponderCreator;
        private readonly Func<ArtistInformationService> mArtistInformationCreator;
        private readonly Func<AlbumInformationService>  mAlbumInformationCreator;
        private readonly Func<TrackInformationService>  mTrackInformationCreator;
        private readonly Func<QueueService>             mQueueControlCreator;
        private readonly Func<QueueStatusResponder>     mQueueStatusResponderCreator;
        private readonly Func<TransportControlService>  mTransportControlServiceCreator;

        public RemoteServiceFactory( Func<HostStatusResponder> createHostStatusResponder, Func<ArtistInformationService> artistInformationCreator,
                                     Func<AlbumInformationService> albumInformationCreator, Func<TrackInformationService> trackInformationCreator,
                                     Func<QueueService> queueControlCreator, Func<QueueStatusResponder> queueStatusResponderCreator,
                                     Func<TransportControlService> transportControlServiceCreator) {
            mHostStatusResponderCreator = createHostStatusResponder;
            mAlbumInformationCreator = albumInformationCreator;
            mArtistInformationCreator = artistInformationCreator;
            mTrackInformationCreator = trackInformationCreator;
            mQueueControlCreator = queueControlCreator;
            mQueueStatusResponderCreator = queueStatusResponderCreator;
            mTransportControlServiceCreator = transportControlServiceCreator;
        }

        public  Grpc.Core.Server            HostServer => new Grpc.Core.Server();

        public  HostStatusResponder         HostStatusResponder => mHostStatusResponderCreator();
        public  ArtistInformationService    ArtistInformationService => mArtistInformationCreator();
        public  AlbumInformationService     AlbumInformationService => mAlbumInformationCreator();
        public  TrackInformationService     TrackInformationService => mTrackInformationCreator();
        public  QueueService                QueueControlService => mQueueControlCreator();
        public  QueueStatusResponder        QueueStatusResponder => mQueueStatusResponderCreator();
        public  TransportControlService     TransportControlService => mTransportControlServiceCreator();
    }
}