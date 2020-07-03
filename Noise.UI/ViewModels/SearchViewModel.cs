using System;
using System.Collections.Generic;
using System.Linq;
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
using ReactiveUI;

namespace Noise.UI.ViewModels {
	public class SearchType {
		public eSearchItemType	ItemType { get; }
		public string			DisplayName { get; }

		public SearchType( eSearchItemType itemType, string displayName ) {
			ItemType = itemType;
			DisplayName = displayName;
		}
	}

	internal class SearchParameters {
		public	eSearchItemType	ItemType { get; }
		public	string			QueryText { get; }

		public SearchParameters( string queryText, eSearchItemType itemType ) {
			ItemType = itemType;
			QueryText = queryText;
        }
    }

	internal class SearchViewModel : ReactiveObject, IDisposable {
		private readonly int									mMaxPlayItems = 10;

		private readonly IEventAggregator						mEventAggregator;
		private readonly IRandomTrackSelector					mTrackSelector;
		private readonly IPlayCommand							mPlayCommand;
		private readonly IPlayQueue								mPlayQueue;
		private readonly ISearchClient							mSearchClient;
		private SearchType										mCurrentSearchType;
		private readonly List<DbTrack>							mApprovalList; 
        private SearchViewNode                                  mSelectedNode;
		private string											mSearchText;
		private CompositeDisposable								mTrash;

	    public  IEnumerable<SearchType>                         SearchTypes { get; }
		public	ObservableCollectionExtended<SearchViewNode>	SearchResults { get; }

		public	ReactiveCommand<Unit, Unit>						PlayRandom { get; }

        public SearchViewModel( IEventAggregator eventAggregator, ISearchProvider searchProvider, IRandomTrackSelector trackSelector, IPlayCommand playCommand, IPlayQueue playQueue ) {
			mEventAggregator = eventAggregator;
			mTrackSelector = trackSelector;
			mPlayCommand = playCommand;
			mPlayQueue = playQueue;

			mApprovalList = new List<DbTrack>();

			mSearchClient = searchProvider.CreateSearchClient();

			mCurrentSearchType = new SearchType( eSearchItemType.Everything, "Everything" );
			SearchTypes = new List<SearchType> { mCurrentSearchType,
												  new SearchType( eSearchItemType.Artist, "Artists" ),
												  new SearchType( eSearchItemType.Album, "Albums" ),
												  new SearchType( eSearchItemType.Track, "Tracks" ),
												  new SearchType( eSearchItemType.BandMember, "Band Members" ),
												  new SearchType( eSearchItemType.Biography, "Biographies" ),
//												  new SearchType( eSearchItemType.Discography, "Discographies" ),
//												  new SearchType( eSearchItemType.Lyrics, "Lyrics" ),
												  new SearchType( eSearchItemType.SimilarArtist, "Similar Artists" ),
												  new SearchType( eSearchItemType.TopAlbum, "Top Albums" ) };

			SearchResults = new ObservableCollectionExtended<SearchViewNode>();
			var searchResultsSubscription = 
                mSearchClient.SearchResults
                    .Transform( r => new SearchViewNode( r, OnPlay ))
                    .ObserveOnDispatcher()
                    .Bind( SearchResults )
                    .Subscribe();

			var clearSearch =
				this
                    .WhenAnyValue( x => x.SearchText )
                    .Where( searchText => String.IsNullOrWhiteSpace( searchText ) || searchText.Length < 3 )
                    .Do( _ => mSearchClient.ClearSearch())
                    .Subscribe();

            var	startSearch =
				this
                    .WhenAnyValue( x => x.SearchText, x => x.CurrentSearchType, ( queryText, searchType ) => new SearchParameters( queryText, searchType.ItemType ))
                    .Where( parameters => !String.IsNullOrWhiteSpace( parameters.QueryText ) && parameters.QueryText.Length > 2 )
                    .Throttle( TimeSpan.FromSeconds( 0.5 ))
                    .Do( StartSearch )
                    .Subscribe();

			var canPlayRandom =
				SearchResults
                    .WhenAnyValue( x => x.Count )
                    .Select( itemCount => itemCount > 10 );

            PlayRandom = ReactiveCommand.Create( OnPlayRandom, canPlayRandom );

			mTrash = new CompositeDisposable( searchResultsSubscription, clearSearch, startSearch, mSearchClient );
		}

		public SearchType CurrentSearchType {
			get => mCurrentSearchType;
		    set => this.RaiseAndSetIfChanged( ref mCurrentSearchType, value );
		}

	    public string SearchText {
			get => mSearchText;
			set => this.RaiseAndSetIfChanged( ref mSearchText, value );
		}

		private void StartSearch( SearchParameters parameters ) {
			mApprovalList.Clear();

            mSearchClient.StartSearch( parameters.ItemType, parameters.QueryText );
        }

		private void OnPlayRandom() {
            mPlayQueue.Add(  mTrackSelector.SelectTracks( from searchItem in SearchResults select searchItem.SearchItem, ApproveTrack, mMaxPlayItems ));
		}

		private bool ApproveTrack( DbTrack track ) {
			bool	retValue = false;

			if((!mPlayQueue.IsTrackQueued( track )) &&
			   ( mApprovalList.FirstOrDefault( t => t.Name.Equals( track.Name, StringComparison.CurrentCultureIgnoreCase )) == null )) {
				mApprovalList.Add( track );

				retValue = true;
			}

			return( retValue );
		}

	    public SearchViewNode SelectedSearchNode {
            get => mSelectedNode;
            set {
                mSelectedNode = value;

                if( mSelectedNode?.Artist != null ) {
                    mEventAggregator.PublishOnUIThread(new Events.ArtistFocusRequested( mSelectedNode.Artist.DbId ));
                }
                if( mSelectedNode?.Album != null ) {
                    mEventAggregator.PublishOnUIThread(new Events.AlbumFocusRequested( mSelectedNode.Album ));
                }
            }
        }

		private void OnPlay( SearchViewNode node ) {
			if( node.Track != null ) {
				mPlayCommand.Play( node.Track );
			}
			else if( node.Album != null ) {
				mPlayCommand.Play( node.Album );
			}
		}

        public void Dispose() {
			mTrash?.Dispose();
			mTrash = null;
        }
    }
}
