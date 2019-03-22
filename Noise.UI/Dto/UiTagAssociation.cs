using System;
using Noise.Infrastructure.Dto;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Dto {
    class UiTagAssociation : AutomaticCommandBase {
        private readonly Action<UiTagAssociation>   mOnPlay;
        private readonly Action<UiTagAssociation>   mOnDelete;

        public DbTagAssociation     Association { get; }
        public DbArtist             Artist { get; }
        public DbAlbum              Album { get; }
        public DbTrack              Track { get; }

        public UiTagAssociation( DbTagAssociation association, DbArtist artist, DbAlbum album, DbTrack track, Action<UiTagAssociation> onPlay, Action<UiTagAssociation> onDelete ) {
            Association = association;
            Artist = artist;
            Album = album;
            Track = track;

            mOnPlay = onPlay;
            mOnDelete = onDelete;
        }

        public string DisplayName => $"{Track.Name} ({Artist.Name}/{Album.Name})";

        public void Execute_Play() {
            mOnPlay?.Invoke( this );
        }

        public void Execute_Delete() {
            mOnDelete?.Invoke( this );
        }
    }
}
