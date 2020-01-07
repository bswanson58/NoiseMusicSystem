using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Caliburn.Micro;
using DynamicData;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;
using Noise.UI.Interfaces;
using ReactiveUI;

namespace Noise.UI.ViewModels {
    class PlaybackRelatedViewModel : ReactiveObject, IDisposable {
        private readonly ITrackProvider                     mTrackProvider;
        private readonly ISearchProvider					mSearchProvider;
        private readonly IPlayCommand						mPlayCommand;
        private readonly IEventAggregator					mEventAggregator;
        private readonly IDisposable                        mSubscriptions;
        private readonly ReactiveCommand<PlayingItem, Unit> mStartSearch;

        public	ObservableCollectionEx<RelatedTrackParent>  Tracks { get; }

        public PlaybackRelatedViewModel( ISelectionState selectionState, ISearchProvider searchProvider, ITrackProvider trackProvider, IPlayCommand playCommand,
                                         IEventAggregator eventAggregator ) {
            mSearchProvider = searchProvider;
            mTrackProvider = trackProvider;
            mPlayCommand = playCommand;
            mEventAggregator = eventAggregator;

            Tracks = new ObservableCollectionEx<RelatedTrackParent>();

            var searchResultsSubscription = 
                mSearchProvider.SearchResults
                    .ObserveOnDispatcher()
                    .Do( AddSearchItem )
                    .Subscribe();

            mStartSearch = ReactiveCommand.CreateFromObservable<PlayingItem, Unit>( item => OnStartSearch( item ).SubscribeOn( RxApp.TaskpoolScheduler ));

            var selectionStateSubscription = selectionState.PlayingTrackChanged.Subscribe( OnTrackStarted );

            mEventAggregator.Subscribe( this );

            mSubscriptions = new CompositeDisposable( selectionStateSubscription, searchResultsSubscription );
        }

        private void AddSearchItem( IChangeSet<SearchResultItem> items ) {
            foreach( var change in items ) {
                if( change.Reason == ListChangeReason.Clear ) {
                    Tracks.Clear();
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
            var parent = Tracks.FirstOrDefault( i => i.Key.Equals( item.Track.Name ));

            if( parent != null ) {
                parent.AddAlbum( item.Artist, item.Album, item.Track );
            }
            else {
                Tracks.Add( new RelatedTrackParent( item.Artist, item.Album, item.Track, OnPlay ));
            }
        }

        private void OnTrackStarted( PlayingItem playingItem ) {
            mStartSearch.Execute( playingItem );
        }

        private IObservable<Unit> OnStartSearch( PlayingItem item ) {
            return Observable.Start( () => {
                if( item.Track != Constants.cDatabaseNullOid ) {
                    var track = mTrackProvider.GetTrack( item.Track );

                    if( track != null ) {
                        mSearchProvider.StartSearch( eSearchItemType.Track , track.Name );
                    }
                }

                return Unit.Default;
            });
        }

        private void OnPlay( RelatedTrackNode node ) { 
            if( node.Track != null ) {
                mPlayCommand.Play( node.Track );
            }
        }

        public void Dispose() {
            mSubscriptions?.Dispose();

            mEventAggregator.Unsubscribe( this );
        }
    }
}
