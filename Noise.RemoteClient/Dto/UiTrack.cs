using System;
using Noise.RemoteServer.Protocol;
using Prism.Commands;
using Prism.Mvvm;

namespace Noise.RemoteClient.Dto {
    class UiTrack : BindableBase {
        private readonly Action<UiTrack>    mPlayAction;

        public  TrackInfo                   Track {  get; }
        public  string                      TrackName => Track.TrackName;
        public  string                      AlbumName => Track.AlbumName;
        public  string                      ArtistName => Track.ArtistName;
        public  TimeSpan                    TrackDuration => TimeSpan.FromMilliseconds( Track.Duration );

        public  DelegateCommand Play { get; }

        public UiTrack( TrackInfo track, Action<UiTrack> onPlay ) {
            Track = track;
            mPlayAction = onPlay;

            Play = new DelegateCommand( OnPlay );
        }

        private void OnPlay() {
            mPlayAction?.Invoke( this );
        }
    }
}
