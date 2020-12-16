using System;
using Noise.RemoteServer.Protocol;
using Prism.Mvvm;

namespace Noise.RemoteClient.Dto {
    class UiQueuedTrack : BindableBase {
        public  QueueTrackInfo  Track { get; }
        public  string          TrackName => Track.TrackName;
        public  string          AlbumName => Track.AlbumName;
        public  string          ArtistName => Track.ArtistName;
        public  TimeSpan        TrackDuration => TimeSpan.FromMilliseconds( Track.Duration );
        public  bool            IsPlaying => Track.IsPlaying;
        public  bool            IsStrategyQueued => Track.IsStrategyQueued;
        public  bool            HasPlayed => Track.HasPlayed;
        public  bool            IsFavorite => Track.IsFavorite;

        public  bool            CanReplay => Track.HasPlayed && !Track.IsPlaying;
        public  bool            CanSkip => !Track.HasPlayed && !Track.IsPlaying;

        public UiQueuedTrack( QueueTrackInfo track ) {
            Track = track;
        }
    }
}
