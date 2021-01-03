using System;
using System.Diagnostics;
using System.Linq;
using Noise.RemoteServer.Protocol;
using Prism.Mvvm;
using Xamarin.Forms.Internals;

namespace Noise.RemoteClient.Dto {
    [DebuggerDisplay("Track = {" + nameof(TrackName) + "}")]
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

        public UiQueuedTrack( TransportInformation fromStatus ) {
            Track = new QueueTrackInfo {
                ArtistId = fromStatus.ArtistId, AlbumId = fromStatus.AlbumId, TrackId = fromStatus.TrackId,
                ArtistName = fromStatus.ArtistName, AlbumName = fromStatus.AlbumName, TrackName = fromStatus.TrackName, VolumeName = fromStatus.VolumeName,
                TrackNumber = fromStatus.TrackNumber, IsFavorite = fromStatus.IsFavorite, Rating = fromStatus.Rating,
                IsPlaying = true
            };

            Track.Tags.AddRange( from t in fromStatus.Tags select new QueueTagInfo{ TagId = t.TagId, TagName = t.TagName });
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

        public void UpdateRatings( TrackInfo fromTrack ) {
            Track.IsFavorite = fromTrack.IsFavorite;
            Track.Rating = fromTrack.Rating;

            RaisePropertyChanged( nameof( IsFavorite ));
            RaisePropertyChanged( nameof( Rating ));
            RaisePropertyChanged( nameof( HasRating ));
            RaisePropertyChanged( nameof( RatingSource ));
        }

        public void UpdateTags( TrackInfo fromTrack ) {
            Track.Tags.Clear();

            fromTrack.Tags.ForEach( t => {
                Track.Tags.Add( new QueueTagInfo{ TagId = t.TagId, TagName = t.TagName });
            });

            RaisePropertyChanged( nameof( Tags ));
        }
    }
}
