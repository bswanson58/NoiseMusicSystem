using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Dto {
    public class UiTrackAlbum : AutomaticCommandBase, IPlayingItem {
        private readonly DbAlbum        mAlbum;
        private readonly DbTrack        mTrack;
        private readonly Action<long>   mPlayAction;

        public  DbTrack                 Track => mTrack;
        public  string                  TrackName => mTrack.Name;
        public  string                  AlbumName => mAlbum.Name;
        public  int                     SortRating => mTrack.IsFavorite ? 6 : mTrack.Rating;
        public                          bool IsPlaying { get; private set; }

        public UiTrackAlbum( DbAlbum album, DbTrack track, Action<long> onPlay ) {
            mAlbum = album;
            mTrack = track;
            mPlayAction = onPlay;
        }

        public void Execute_Play() {
            // trigger the track queue animation
            RaisePropertyChanged( "AnimateQueueTrack" );

            mPlayAction?.Invoke( Track.DbId );
        }

        public void SetPlayingStatus( PlayingItem item ) {
            IsPlaying = item.Track.Equals( Track.DbId );

            RaisePropertyChanged( () => IsPlaying );
        }
    }
}
