using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Dto {
    class UiTagAssociation : AutomaticPropertyBase, IPlayingItem {
        private readonly Action<UiTagAssociation>   mOnPlay;
        private readonly Action<UiTagAssociation>   mOnDelete;

        public  DbTagAssociation    Association { get; }
        public  DbArtist            Artist { get; }
        public  DbAlbum             Album { get; }
        public  DbTrack             Track { get; }

        public  DelegateCommand     Play { get; }
        public  DelegateCommand     Delete { get; }

        public UiTagAssociation( DbTagAssociation association, DbArtist artist, DbAlbum album, DbTrack track, Action<UiTagAssociation> onPlay, Action<UiTagAssociation> onDelete ) {
            Association = association;
            Artist = artist;
            Album = album;
            Track = track;

            mOnPlay = onPlay;
            mOnDelete = onDelete;

            Play = new DelegateCommand( OnPlay );
            Delete = new DelegateCommand( OnDelete );
        }

        public string DisplayName => $"{Track.Name} ({Artist.Name}/{Album.Name})";

        public bool IsPlaying {
            get { return( Get( () => IsPlaying )); }
            set {  Set( () => IsPlaying, value ); }
        }

        public void SetPlayingStatus( PlayingItem item ) {
            IsPlaying = Track.DbId.Equals( item.Track );
        }

        private void OnPlay() {
            // trigger the track queue animation
            RaisePropertyChanged( "AnimateQueueTrack" );

            mOnPlay?.Invoke( this );
        }

        private void OnDelete() {
            mOnDelete?.Invoke( this );
        }
    }
}
