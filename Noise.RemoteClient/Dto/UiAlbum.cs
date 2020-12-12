using System;
using Noise.RemoteServer.Protocol;
using Prism.Commands;

namespace Noise.RemoteClient.Dto {
    class UiAlbum {
        private readonly Action<UiAlbum>    mPlayAction;

        public  AlbumInfo                   Album { get; }
        public  string                      AlbumName => Album.AlbumName;
        public  Int32                       TrackCount => Album.TrackCount;
        public  Int32                       PublishedYear => Album.PublishedYear;
        public  bool                        HasPublishedYear => Album.PublishedYear > 0;

        public  DelegateCommand             Play { get; }

        public UiAlbum( AlbumInfo album, Action<UiAlbum> onPlay ) {
            Album = album;
            mPlayAction = onPlay;

            Play = new DelegateCommand( OnPlay );
        }

        private void OnPlay() {
            mPlayAction?.Invoke( this );
        }
    }
}
