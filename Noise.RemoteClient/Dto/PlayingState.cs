using System;
using Noise.RemoteClient.Support;

namespace Noise.RemoteClient.Dto {
    class PlayingState {
        public  Int64       ArtistId { get; }
        public  Int64       AlbumId { get; }
        public  Int64       TrackId { get; }
        public  string      ArtistName { get; }
        public  string      AlbumName { get; }
        public  string      TrackName { get; }

        public PlayingState( UiQueuedTrack forTrack ) {
            ArtistId = forTrack.Track.ArtistId;
            AlbumId = forTrack.Track.AlbumId;
            TrackId = forTrack.Track.TrackId;

            ArtistName = forTrack.ArtistName;
            AlbumName = forTrack.AlbumName;
            TrackName = forTrack.TrackName;
        }

        public PlayingState() {
            ArtistId = Constants.cNullId;
            AlbumId = Constants.cNullId;
            TrackId = Constants.cNullId;

            ArtistName = String.Empty;
            AlbumName = String.Empty;
            TrackName = String.Empty;
        }
    }
}
