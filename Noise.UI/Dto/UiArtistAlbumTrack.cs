using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Dto {
    public class UiArtistAlbumTrack : PropertyChangeBase {
        private readonly Action<DbTrack>    mTrackPlayAction;
        private readonly List<string>       mTags;
        
        public  DbArtist                    Artist { get; }
        public  DbAlbum                     Album { get; }
        public  DbTrack                     Track {  get; }

        public  string                      ArtistName => Artist.Name;
        public  string                      AlbumName => Album.Name;
        public  string                      TrackName => Track.Name;
        public  TimeSpan                    Duration => Track.Duration;

        public  bool                        HasTags => mTags.Any();
        public string                       TagsTooltip => mTags.Any() ? string.Join( Environment.NewLine, mTags ) : "Associated Track Tags";

        public  DelegateCommand             Play { get; }

        public UiArtistAlbumTrack( DbArtist artist, DbAlbum album, DbTrack track, Action<DbTrack> onPlay ) {
            Artist = artist;
            Album = album;
            Track = track;
            mTrackPlayAction = onPlay;

            mTags = new List<string>();
            Play = new DelegateCommand( OnPlay );
        }

        public void SetTags( IEnumerable<string> tags ) {
            mTags.Clear();
            mTags.AddRange( tags );

            RaisePropertyChanged( () => HasTags );
            RaisePropertyChanged( () => TagsTooltip );
        }

        private void OnPlay() {
            mTrackPlayAction?.Invoke( Track );
        }
    }
}
