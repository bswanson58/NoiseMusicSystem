using System;

namespace Noise.RemoteClient.Dto {
    class SuggestionState {
        public  Int64       ArtistId { get; }
        public  Int64       AlbumId { get; }
        public  Int64       TrackId { get; }
        public  string      ArtistName { get; }
        public  string      AlbumName { get; }
        public  string      TrackName { get; }

        public SuggestionState( UiQueuedTrack forTrack ) {
            ArtistId = forTrack.Track.ArtistId;
            AlbumId = forTrack.Track.AlbumId;
            TrackId = forTrack.Track.TrackId;

            ArtistName = forTrack.ArtistName;
            AlbumName = forTrack.AlbumName;
            TrackName = forTrack.TrackName;
        }
    }
}
