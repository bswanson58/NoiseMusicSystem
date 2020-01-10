using System;
using System.ComponentModel;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Dto {
    public class RelatedTrackNode : AutomaticCommandBase, IPlayingItem {
        protected readonly Action<RelatedTrackNode> OnPlay;
        private bool                                mIsExpanded;

        public  string      Key { get; }
        public  DbArtist    Artist { get; }
        public  DbAlbum     Album { get; }
        public  DbTrack     Track { get; }
        public  string      AlbumName => $"{Artist.Name}/{Album.Name}";
        public  bool        DisplayTrackName { get; set; }
        public  bool        IsPlaying { get; private set; }

        public RelatedTrackNode( string key, DbArtist artist, DbAlbum album, DbTrack track, Action<RelatedTrackNode> onPlay ) {
            Key = key;
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
                RaisePropertyChanged( "FirstAlbumName" );
            }
        }

        public void Execute_Play() {
            OnPlay?.Invoke( this );
        }

        public void SetPlayingStatus( PlayingItem item ) {
            IsPlaying = Track.DbId.Equals( item.Track );

            RaisePropertyChanged( () => IsPlaying );
        }
    }

    public class RelatedTrackParent : RelatedTrackNode {
        private readonly string                             mParentName;

        public	string		                                FirstAlbumName => MultipleTracks ? IsExpanded ? "Album List:" : $" (on {Tracks.Count} albums - expand to view list)" : AlbumName;
        public  string                                      ParentName => String.IsNullOrWhiteSpace( mParentName ) ? Track.Name : mParentName;
        public  string                                      SortKey => String.IsNullOrWhiteSpace( mParentName ) ? Track.Name : $"Z - {mParentName}";
        public	ObservableCollectionEx<RelatedTrackNode>    Tracks { get; }
        public  bool                                        IsPlayable => Tracks.Count == 0;
        public	bool		                                MultipleTracks => Tracks.Count > 0;
        public  bool                                        IsCategoryParent => !String.IsNullOrWhiteSpace( mParentName );

        public RelatedTrackParent( string key, DbArtist artist, DbAlbum album, DbTrack track, Action<RelatedTrackNode> onPlay, bool expanded ) :
            base( key, artist, album, track, onPlay ) {
            IsExpanded = expanded;

            mParentName = String.Empty;
            Tracks = new ObservableCollectionEx<RelatedTrackNode>();
        }

        public RelatedTrackParent( string key, DbArtist artist, DbAlbum album, DbTrack track, Action<RelatedTrackNode> onPlay ) :
            this( key, artist, album, track, onPlay, false ) { }

        public RelatedTrackParent( string key, string parentName, DbArtist artist, DbAlbum album, DbTrack track, Action<RelatedTrackNode> onPlay ) :
            this( key, artist, album, track, onPlay, false ) {
            mParentName = parentName;

            AddNode( Artist, Album, Track );
        }

        public void AddAlbum( DbArtist artist, DbAlbum album, DbTrack track ) {
            if(!Tracks.Any()) {
                AddNode( Artist, Album, Track );
            }

            AddNode( artist, album, track );

            RaisePropertyChanged( () => IsPlayable );
            RaisePropertyChanged( () => MultipleTracks );
            RaisePropertyChanged( () => FirstAlbumName );
        }

        private void AddNode( DbArtist artist, DbAlbum album, DbTrack track ) {
            var node = new RelatedTrackNode( Key, artist, album, track, OnPlay ) { DisplayTrackName = !String.IsNullOrWhiteSpace( mParentName ) };

            Tracks.Add( node );
            Tracks.Sort( a => a.AlbumName, ListSortDirection.Ascending );
        }
    }

}
