using System;
using System.ComponentModel;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Dto {
    public class RelatedTrackNode : AutomaticCommandBase {
        protected readonly Action<RelatedTrackNode> OnPlay;
        private bool                                mIsExpanded;

        public  DbArtist    Artist { get; }
        public  DbAlbum     Album { get; }
        public  DbTrack     Track { get; }
        public	string		TrackName => Track.Name;
        public  string      Key => TrackName;
        public  string      AlbumName => $"{Artist.Name}/{Album.Name}";

        public RelatedTrackNode( DbArtist artist, DbAlbum album, DbTrack track, Action<RelatedTrackNode> onPlay ) {
            Artist = artist;
            Album = album;
            Track = track;

            OnPlay = onPlay;
        }

        public bool IsExpanded {
            get => mIsExpanded;
            set {
                mIsExpanded = value;

                RaisePropertyChanged( () => IsExpanded );
            }
        }

        public void Execute_Play() {
            OnPlay?.Invoke( this );
        }
    }

    public class RelatedTrackParent : RelatedTrackNode {
        public	string		                                FirstAlbumName => MultipleTracks ? IsExpanded ? "Album List:" : $" (on {Tracks.Count} albums - expand to view list)" : AlbumName;
        public  string                                      ParentName => Track.Name;
        public	ObservableCollectionEx<RelatedTrackNode>    Tracks { get; }
        public  bool                                        IsPlayable => Tracks.Count == 0;
        public	bool		                                MultipleTracks => Tracks.Count > 0;

        public RelatedTrackParent( DbArtist artist, DbAlbum album, DbTrack track, Action<RelatedTrackNode> onPlay ) :
            base( artist, album, track, onPlay ) {
            Tracks = new ObservableCollectionEx<RelatedTrackNode>();
        }

        public void AddAlbum( DbArtist artist, DbAlbum album, DbTrack track ) {
            if(!Tracks.Any()) {
                Tracks.Add( new RelatedTrackNode( Artist, Album, Track, OnPlay ));
            }

            Tracks.Add( new RelatedTrackNode( artist, album, track, OnPlay ));
            Tracks.Sort( a => a.AlbumName, ListSortDirection.Ascending );

            RaisePropertyChanged( () => IsPlayable );
            RaisePropertyChanged( () => MultipleTracks );
            RaisePropertyChanged( () => FirstAlbumName );
        }
    }
}
