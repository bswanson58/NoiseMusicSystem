using System;
using System.Diagnostics;
using Noise.RemoteServer.Protocol;
using Prism.Commands;
using Prism.Mvvm;

namespace Noise.RemoteClient.Dto {
    [DebuggerDisplay("Track = {" + nameof(AlbumName) + "}")]
    class UiAlbum : BindableBase {
        private readonly Action<UiAlbum>    mPlayAction;
        private bool                        mIsPlaying;

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

        public bool IsPlaying {
            get => mIsPlaying;
            set => SetProperty( ref mIsPlaying, value );
        }

        public void SetIsPlaying( PlayingState state ) {
            IsPlaying = Album.AlbumId.Equals( state?.AlbumId );
        }

        public void UpdateRatings( AlbumInfo fromAlbum ) {
            Album.IsFavorite = fromAlbum.IsFavorite;
            Album.Rating = fromAlbum.Rating;

            RaisePropertyChanged( nameof( IsFavorite ));
            RaisePropertyChanged( nameof( Rating ));
            RaisePropertyChanged( nameof( HasRating ));
            RaisePropertyChanged( nameof( RatingSource ));
        }

        private void OnPlay() {
            mPlayAction?.Invoke( this );
        }
    }
}
