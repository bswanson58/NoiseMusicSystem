using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Dto {
    class UiQueuedTrack {
        public  QueueTrackInfo  Track { get; }
        public  string          TrackName => Track.TrackName;
        public  string          AlbumName => Track.AlbumName;
        public  string          ArtistName => Track.ArtistName;

        public UiQueuedTrack( QueueTrackInfo track ) {
            Track = track;
        }
    }
}
