using System;
using System.Linq;
using Noise.RemoteServer.Protocol;
using Prism.Commands;
using Prism.Mvvm;

namespace Noise.RemoteClient.Dto {
    class UiTrack : BindableBase {
        private readonly Action<UiTrack>    mPlayAction;

        public  TrackInfo                   Track { get; }
        public  string                      TrackName => Track.TrackName;
        public  string                      AlbumName => Track.AlbumName;
        public  string                      ArtistName => Track.ArtistName;
        public  TimeSpan                    TrackDuration => TimeSpan.FromMilliseconds( Track.Duration );
        public  bool                        IsFavorite => Track.IsFavorite;
        public  Int32                       Rating => Track.Rating;
        public  bool                        HasRating => Rating != 0;
        public  string                      Tags => String.Join( " | ", from t in Track.Tags select t.TagName );

        public  DelegateCommand Play { get; }

        public UiTrack( TrackInfo track, Action<UiTrack> onPlay ) {
            Track = track;
            mPlayAction = onPlay;

            Play = new DelegateCommand( OnPlay );
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

        private void OnPlay() {
            mPlayAction?.Invoke( this );
        }
    }
}
