using System;
using System.Linq;
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
        public  string          Tags => String.Join( " | ", from t in Track.Tags select t.TagName );
        public  Int32           Rating => Track.Rating;
        public  bool            HasRating => Rating != 0;

        public  bool            CanReplay => Track.HasPlayed && !Track.IsPlaying;
        public  bool            CanSkip => !Track.HasPlayed && !Track.IsPlaying;

        public UiQueuedTrack( QueueTrackInfo track ) {
            Track = track;
        }

        public string RatingSource {
            get {
                var retValue = "0_Star";

                if(( Rating > 0 ) &&
                   ( Rating < 6 )) {
                    retValue = $"{Rating:D1}_Star";
                }

                return retValue;
            }
        }
    }
}
