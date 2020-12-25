using System;
using System.Diagnostics;
using System.Linq;
using Noise.RemoteServer.Protocol;
using Prism.Commands;
using Prism.Mvvm;

namespace Noise.RemoteClient.Dto {
    [DebuggerDisplay("Track = {" + nameof(TrackName) + "}")]
    class UiTrack : BindableBase {
        private readonly Action<UiTrack, bool>  mPlayAction;
        private bool                            mIsPlaying;

        public  TrackInfo                   Track { get; }
        public  string                      TrackName => Track.TrackName;
        public  string                      AlbumName => Track.AlbumName;
        public  string                      ArtistName => Track.ArtistName;
        public  TimeSpan                    TrackDuration => TimeSpan.FromMilliseconds( Track.Duration );
        public  bool                        IsFavorite => Track.IsFavorite;
        public  Int32                       Rating => Track.Rating;
        public  bool                        HasRating => Rating != 0;
        public  string                      Tags => String.Join( " | ", from t in Track.Tags select t.TagName );

        public  DelegateCommand             Play { get; }
        public  DelegateCommand             PlayNext { get; }

        public UiTrack( TrackInfo track, Action<UiTrack, bool> onPlay, PlayingState state ) :
            this( track, onPlay ) {
            SetIsPlaying( state );
        }

        public UiTrack( TrackInfo track, Action<UiTrack, bool> onPlay ) {
            Track = track;
            mPlayAction = onPlay;

            Play = new DelegateCommand( OnPlay );
            PlayNext = new DelegateCommand( OnPlayNext );
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

        public bool IsPlaying {
            get => mIsPlaying;
            set => SetProperty( ref mIsPlaying, value );
        }

        public void SetIsPlaying( PlayingState state ) {
            IsPlaying = Track.TrackId.Equals( state?.TrackId );
        }

        private void OnPlay() {
            mPlayAction?.Invoke( this, false );
        }

        private void OnPlayNext() {
            mPlayAction?.Invoke( this, true );
        }
    }
}
