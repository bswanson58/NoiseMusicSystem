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
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Prism;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;
using Noise.UI.Interfaces;
using ReactiveUI;
using ReusableBits.ExtensionClasses;

namespace Noise.UI.ViewModels {
    class PlaybackRelatedViewModel : ReactiveObject, IActiveAware, IDisposable,
                                     IHandle<Events.DatabaseClosing>{
        private readonly IArtistProvider                    mArtistProvider;
        private readonly IAlbumProvider                     mAlbumProvider;
        private readonly ITrackProvider                     mTrackProvider;
        private readonly ISelectionState                    mSelectionState;
        private readonly ISearchClient                      mSearchClient;
        private readonly IPlayCommand						mPlayCommand;
        private readonly IEventAggregator					mEventAggregator;
        private readonly IDisposable                        mSubscriptions;
        private readonly ReactiveCommand<PlayingItem, Unit> mStartSearch;
        private readonly ObservableCollectionEx<RelatedTrackParent> mTracks;
        private IDisposable                                 mSelectionStateSubscription;
        private bool                                        mIsActive;
        private bool                                        mRelatedTracksAvailable;

        public  ICollectionView                             Tracks { get; }
        public  ReactiveCommand<RelatedTrackNode, Unit>     TreeViewSelected { get; }
        public	event EventHandler				            IsActiveChanged  = delegate { };

        public PlaybackRelatedViewModel( ISelectionState selectionState, ISearchProvider searchProvider, IPlayCommand playCommand,
                                         IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, IEventAggregator eventAggregator ) {
            mArtistProvider = artistProvider;
            mAlbumProvider = albumProvider;
            mTrackProvider = trackProvider;
            mSelectionState = selectionState;
            mPlayCommand = playCommand;
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

            mStartSearch = ReactiveCommand.CreateFromObservable<PlayingItem, Unit>( item => OnStartSearch( item ).SubscribeOn( RxApp.TaskpoolScheduler ));

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
                        mSelectionStateSubscription = mSelectionState.PlayingTrackChanged.Subscribe( OnTrackStarted );

                        OnTrackStarted( mSelectionState.CurrentlyPlayingItem );
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
            var key = item.Track.Name.RemoveSpecialCharacters().ToLower();
            var parent = mTracks.FirstOrDefault( i => i.Key.Equals( key ));

            if( parent != null ) {
                parent.AddAlbum( item.Artist, item.Album, item.Track );
            }
            else {
                var expanded = !mTracks.Any();

                mTracks.Add( new RelatedTrackParent( key, item.Artist, item.Album, item.Track, OnPlay, expanded ));
            }
        }

        private void OnTrackStarted( PlayingItem playingItem ) {
            mStartSearch.Execute( playingItem );
        }

        private IObservable<Unit> OnStartSearch( PlayingItem item ) {
            return Observable.Start( () => {
                if(( item != null ) &&
                   ( item.Track != Constants.cDatabaseNullOid )) {
                    var track = mTrackProvider.GetTrack( item.Track );

                    if( track != null ) {
                        var searchText = $"\"{track.Name}\"";

                        mSearchClient.StartSearch( eSearchItemType.Track , searchText );

                        AddArtistFavoriteTracks( track.Artist );
                    }
                }

                return Unit.Default;
            });
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

            mEventAggregator.Unsubscribe( this );
        }
    }
}
