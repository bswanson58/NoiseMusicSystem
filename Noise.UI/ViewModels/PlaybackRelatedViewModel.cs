using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using DynamicData;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;
using Noise.UI.Interfaces;
using Prism;
using ReactiveUI;
using ReusableBits.ExtensionClasses;
using ReusableBits.ExtensionClasses.MoreLinq;

namespace Noise.UI.ViewModels {
    class PlaybackRelatedViewModel : ReactiveObject, IActiveAware, IDisposable,
                                     IHandle<Events.DatabaseClosing>{
        private readonly IArtistProvider                    mArtistProvider;
        private readonly IAlbumProvider                     mAlbumProvider;
        private readonly ITrackProvider                     mTrackProvider;
        private readonly ISelectionState                    mSelectionState;
        private readonly ISearchClient                      mSearchClient;
        private readonly IPlayCommand						mPlayCommand;
        private readonly IPlayingItemHandler                mPlayingItemHandler;
        private readonly IEventAggregator					mEventAggregator;
        private readonly IDisposable                        mSubscriptions;
        private readonly ReactiveCommand<DbTrack, Unit>     mStartSearch;
        private readonly ObservableCollectionEx<RelatedTrackParent> mTracks;
        private DbTrack                                     mCurrentTrack;
        private IDisposable                                 mSelectionStateSubscription;
        private bool                                        mIsActive;
        private bool                                        mRelatedTracksAvailable;

        public  ICollectionView                             Tracks { get; }
        public  string                                      Title => mCurrentTrack != null ? $"Tracks Related to '{mCurrentTrack.Name}'" : " Playback Related Tracks ";
        public  ReactiveCommand<RelatedTrackNode, Unit>     TreeViewSelected { get; }
        public	event EventHandler				            IsActiveChanged  = delegate { };

        public PlaybackRelatedViewModel( ISelectionState selectionState, ISearchProvider searchProvider, IPlayCommand playCommand, IPlayingItemHandler playingItemHandler,
                                         IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, IEventAggregator eventAggregator ) {
            mArtistProvider = artistProvider;
            mAlbumProvider = albumProvider;
            mTrackProvider = trackProvider;
            mSelectionState = selectionState;
            mPlayCommand = playCommand;
            mPlayingItemHandler = playingItemHandler;
            mEventAggregator = eventAggregator;

            mTracks = new ObservableCollectionEx<RelatedTrackParent>();
            mTracks.CollectionChanged += OnCollectionChanged;
            TreeViewSelected = ReactiveCommand.Create<RelatedTrackNode, Unit>( OnTreeViewSelection );

            Tracks = CollectionViewSource.GetDefaultView( mTracks );
            Tracks.SortDescriptions.Add( new SortDescription( nameof( RelatedTrackParent.SortKey ), ListSortDirection.Ascending ));

            mSearchClient = searchProvider.CreateSearchClient();

            var searchResultsSubscription = 
                mSearchClient.SearchResults
                    .ObserveOnDispatcher()
                    .Do( AddSearchItem )
                    .Subscribe();

            mStartSearch = ReactiveCommand.CreateFromObservable<DbTrack, Unit>( item => OnStartSearch( item ).SubscribeOn( RxApp.TaskpoolScheduler ));
            mPlayingItemHandler.StartHandler();

            mEventAggregator.Subscribe( this );

            mSubscriptions = new CompositeDisposable( searchResultsSubscription, mSearchClient );
        }

        private void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs args ) {
            RelatedTracksAvailable = mTracks.Any();
        }

        public bool RelatedTracksAvailable {
            get => mRelatedTracksAvailable;
            set => this.RaiseAndSetIfChanged( ref mRelatedTracksAvailable, value, nameof( RelatedTracksAvailable ));
        }

        public bool IsActive {
            get => mIsActive;
            set {
                mIsActive = value;

                if( mIsActive ) {
                    if( mSelectionStateSubscription == null ) {
                        mSelectionStateSubscription = mSelectionState.FocusChanged.Subscribe( OnTrackStarted );

                        OnTrackStarted( mSelectionState.CurrentFocus );
                    }
                }
                else {
                    mSelectionStateSubscription?.Dispose();
                    mSelectionStateSubscription = null;
                }

                IsActiveChanged( this, new EventArgs());
            }
        }

        public void Handle( Events.DatabaseClosing args ) {
            mSearchClient.ClearSearch();
            mTracks.Clear();
        }

        private void AddSearchItem( IChangeSet<SearchResultItem> items ) {
            foreach( var change in items ) {
                if( change.Reason == ListChangeReason.Clear ) {
                    mTracks.Clear();
                }
                else if( change.Reason == ListChangeReason.Add ) {
                    AddSearchItem( change.Item.Current );
                }
                else if( change.Reason == ListChangeReason.AddRange ) {
                    foreach( var  item in change.Range ) {
                        AddSearchItem( item );
                    }
                }
            }
        }

        private void AddSearchItem( SearchResultItem item ) {
            if(( item?.Track != null ) &&
               ( item.Album != null ) &&
               ( item.Artist != null )) {
                var key = item.Track.Name.RemoveSpecialCharacters().ToLower();
                var parent = mTracks.FirstOrDefault( i => i.Key.Equals( key ));

                if( parent != null ) {
                    parent.AddAlbum( item.Artist, item.Album, item.Track );

                    mPlayingItemHandler.UpdateList( parent.TrackList );
                }
                else {
                    parent = new RelatedTrackParent( key, item.Artist, item.Album, item.Track, OnPlay );
                    mPlayingItemHandler.UpdateItem( parent );

                    mTracks.Add( parent );
                }
            }
        }

        private void OnTrackStarted( DbTrack track ) {
            mCurrentTrack = track;

            mStartSearch.Execute( track );

            this.RaisePropertyChanged( nameof( Title ));
        }

        private IObservable<Unit> OnStartSearch( DbTrack track ) {
            return Observable.Start( () => {
                if( track != null ) {
                    mSearchClient.StartSearch( eSearchItemType.Track , CreateSearchTerm( track.Name ));

                    AddArtistFavoriteTracks( track.Artist );
                }

                return Unit.Default;
            });
        }

        private string CreateSearchTerm( string input ) {
            var retValue = DeleteText( input, '(', ')' );

            retValue = DeleteText( retValue, '[', ']' );
            retValue = retValue.Trim();
            retValue = $"\"{retValue}\"";

            return String.IsNullOrWhiteSpace( retValue ) ? input : retValue;
        }

        private string DeleteText( string source, char startCharacter, char endCharacter ) {
            var     retValue = source;
            bool    textDeleted;

            do {
                var startPosition = retValue.IndexOf( startCharacter );
                var endPosition = retValue.IndexOf( endCharacter );

                if(( startPosition >= 0 ) &&
                   ( endPosition > startPosition )) {
                    retValue = retValue.Remove( startPosition, endPosition - startPosition + 1 );

                    textDeleted = true;
                }
                else {
                    textDeleted = false;
                }
            } while( textDeleted );

            return retValue;
        }

        private void AddArtistFavoriteTracks( long artistId ) {
            var parentNode = default( RelatedTrackParent );

            using( var trackList = mTrackProvider.GetFavoriteTracks()) {
                var artistTracks = from t in trackList.List where t.Artist.Equals( artistId ) select t;

                artistTracks.ForEach( track => {
                    var artist = mArtistProvider.GetArtist( track.Artist );
                    var album = mAlbumProvider.GetAlbum( track.Album );

                    if(( artist != null ) &&
                       ( album != null )) {
                        if( parentNode != null ) {
                            parentNode.AddAlbum( artist, album, track );
                        }
                        else {
                            parentNode = new RelatedTrackParent( "|favorites|", "Favorites", artist, album, track, OnPlay );
                        }
                    }
                });
            }

            if( parentNode != null ) {
                Execute.OnUIThread( () => mTracks.Add( parentNode ));

                mPlayingItemHandler.UpdateList( parentNode.TrackList );
            }
        }

        private void OnPlay( RelatedTrackNode node ) { 
            if( node.Track != null ) {
                mPlayCommand.Play( node.Track );
            }
        }

        private Unit OnTreeViewSelection( RelatedTrackNode node ) {
            if( node != null ) {
                if( node is RelatedTrackParent parent ) {
                    if( parent.IsPlayable ) {
                        mEventAggregator.PublishOnUIThread( new Events.AlbumFocusRequested( node.Album ));
                    }
                }
                else {
                    mEventAggregator.PublishOnUIThread( new Events.AlbumFocusRequested( node.Album ));
                }
            }

            return Unit.Default;
        }

        public void Dispose() {
            mSubscriptions?.Dispose();

            mSelectionStateSubscription?.Dispose();
            mSelectionStateSubscription = null;

            mPlayingItemHandler.StopHandler();

            mEventAggregator.Unsubscribe( this );
        }
    }
}
