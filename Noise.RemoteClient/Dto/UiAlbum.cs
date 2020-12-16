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
        public  bool                        IsFavorite => Album.IsFavorite;
        public  Int32                       Rating => Album.Rating;
        public  bool                        HasRating => Rating != 0;

        public  DelegateCommand             Play { get; }

        public UiAlbum( AlbumInfo album, Action<UiAlbum> onPlay ) {
            Album = album;
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
