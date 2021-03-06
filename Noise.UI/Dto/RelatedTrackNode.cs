﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Dto {
    public class RelatedTrackNode : PropertyChangeBase, IPlayingItem {
        protected readonly Action<RelatedTrackNode> OnPlay;
        private readonly RelatedTrackParent         mParent;
        private bool                                mIsExpanded;

        public  string          Key { get; }
        public  DbArtist        Artist { get; }
        public  DbAlbum         Album { get; }
        public  DbTrack         Track { get; }
        public  string          AlbumName => $"{Artist.Name}/{Album.Name}";
        public  bool            DisplayTrackName { get; set; }
        public  string          SortTrackName => DisplayTrackName ? Track.Name : String.Empty;
        public  bool            IsPlaying { get; private set; }
        public  DelegateCommand Play { get; }

        public RelatedTrackNode( string key, DbArtist artist, DbAlbum album, DbTrack track, Action<RelatedTrackNode> onPlay, RelatedTrackParent parent ) : 
            this( key, artist, album, track, onPlay ) {
            mParent = parent;
        }

        public RelatedTrackNode( string key, DbArtist artist, DbAlbum album, DbTrack track, Action<RelatedTrackNode> onPlay ) {
            Key = key;
            Artist = artist;
            Album = album;
            Track = track;

            OnPlay = onPlay;

            Play = new DelegateCommand( OnPlayCommand );
        }

        public bool IsExpanded {
            get => mIsExpanded;
            set {
                mIsExpanded = value;

                RaisePropertyChanged( () => IsExpanded );
                RaisePropertyChanged( "FirstAlbumName" );
            }
        }

        private void OnPlayCommand() {
            // trigger the track queue animation
            RaisePropertyChanged( "AnimateQueueTrack" );

            OnPlay?.Invoke( this );
        }

        public void SetPlayingStatus( PlayingItem item ) {
            IsPlaying = Track.DbId.Equals( item.Track );

            if( IsPlaying ) {
                IsExpanded = true;

                if( mParent != null ) {
                    mParent.IsExpanded = true;
                }
            }

            RaisePropertyChanged( () => IsPlaying );
        }
    }

    public class RelatedTrackParent : RelatedTrackNode {
        private readonly string                                     mParentName;
        private	readonly ObservableCollectionEx<RelatedTrackNode>   mTracks;

        public  ICollectionView                 Tracks { get; }
        public  IReadOnlyList<RelatedTrackNode> TrackList => mTracks;
        public	string		                    FirstAlbumName => MultipleTracks ? IsExpanded ? $"Album List ({mTracks.Count}):" : $" (on {mTracks.Count} albums - expand to view list)" : AlbumName;
        public  string                          ParentName => String.IsNullOrWhiteSpace( mParentName ) ? Track.Name : mParentName;
        public  string                          SortKey => String.IsNullOrWhiteSpace( mParentName ) ? Track.Name : $"Z - {mParentName}";
        public  bool                            IsPlayable => mTracks.Count == 0;
        public	bool		                    MultipleTracks => mTracks.Count > 0;
        public  bool                            IsCategoryParent => !String.IsNullOrWhiteSpace( mParentName );

        public RelatedTrackParent( string key, DbArtist artist, DbAlbum album, DbTrack track, Action<RelatedTrackNode> onPlay ) :
            base( key, artist, album, track, onPlay ) {

            mParentName = String.Empty; 
            mTracks = new ObservableCollectionEx<RelatedTrackNode>();

            Tracks = CollectionViewSource.GetDefaultView( mTracks );
            Tracks.SortDescriptions.Add( new SortDescription( nameof( SortTrackName ), ListSortDirection.Ascending ));
            Tracks.SortDescriptions.Add( new SortDescription( "Track.RatingSortValue", ListSortDirection.Descending ));
            Tracks.SortDescriptions.Add( new SortDescription( "Artist.Name", ListSortDirection.Ascending ));
            Tracks.SortDescriptions.Add( new SortDescription( "Album.Name", ListSortDirection.Ascending ));
        }

        public RelatedTrackParent( string key, string parentName, DbArtist artist, DbAlbum album, DbTrack track, Action<RelatedTrackNode> onPlay ) :
            this( key, artist, album, track, onPlay ) {
            mParentName = parentName;

            AddNode( Artist, Album, Track );
        }

        public void AddAlbum( DbArtist artist, DbAlbum album, DbTrack track ) {
            if(!mTracks.Any()) {
                AddNode( Artist, Album, Track );
            }

            AddNode( artist, album, track );

            RaisePropertyChanged( () => IsPlayable );
            RaisePropertyChanged( () => MultipleTracks );
            RaisePropertyChanged( () => FirstAlbumName );
        }

        private void AddNode( DbArtist artist, DbAlbum album, DbTrack track ) {
            var node = new RelatedTrackNode( Key, artist, album, track, OnPlay, this ) { DisplayTrackName = !String.IsNullOrWhiteSpace( mParentName ) };

            mTracks.Add( node );
        }
    }

}
