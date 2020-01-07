using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Caliburn.Micro;
using DynamicData;
using DynamicData.Binding;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;
using Noise.UI.Interfaces;
using ReactiveUI;

namespace Noise.UI.ViewModels {
    class PlaybackRelatedViewModel : ReactiveObject, IDisposable {
        private readonly ITrackProvider                         mTrackProvider;
        private readonly ISearchProvider						mSearchProvider;
        private readonly IPlayCommand							mPlayCommand;
        private readonly IEventAggregator						mEventAggregator;
        private readonly IDisposable                            mSubscriptions;
        private readonly ReactiveCommand<PlayingItem, Unit>     mStartSearch;

        public	ObservableCollectionExtended<SearchViewNode>	SearchResults { get; }

        public PlaybackRelatedViewModel( ISelectionState selectionState, ISearchProvider searchProvider, ITrackProvider trackProvider, IPlayCommand playCommand,
                                         IEventAggregator eventAggregator ) {
            mSearchProvider = searchProvider;
            mTrackProvider = trackProvider;
            mPlayCommand = playCommand;
            mEventAggregator = eventAggregator;

            SearchResults = new ObservableCollectionExtended<SearchViewNode>();

            var searchResultsSubscription = 
                mSearchProvider.SearchResults
                    .Transform( r => new SearchViewNode( r, OnPlay ))
                    .Filter( n => n.CanPlay )
                    .ObserveOnDispatcher()
                    .Bind( SearchResults )
                    .Subscribe();

            mStartSearch = ReactiveCommand.CreateFromObservable<PlayingItem, Unit>( item => OnStartSearch( item ).SubscribeOn( RxApp.TaskpoolScheduler ));

            var selectionStateSubscription = selectionState.PlayingTrackChanged.Subscribe( OnTrackStarted );

            mEventAggregator.Subscribe( this );

            mSubscriptions = new CompositeDisposable( selectionStateSubscription, searchResultsSubscription );
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

        private void OnPlay( SearchViewNode node ) { 
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
