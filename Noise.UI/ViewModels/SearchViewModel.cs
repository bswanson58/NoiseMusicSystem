using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Caliburn.Micro;
using DynamicData;
using DynamicData.Binding;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;
using Noise.UI.Logging;

namespace Noise.UI.ViewModels {
	public class SearchType {
		public eSearchItemType	ItemType { get; }
		public string			DisplayName { get; }

		public SearchType( eSearchItemType itemType, string displayName ) {
			ItemType = itemType;
			DisplayName = displayName;
		}
	}

	internal class SearchViewModel : ViewModelBase, IDisposable {
		private const int										cMaxPlayItems = 10;

		private readonly IEventAggregator						mEventAggregator;
		private readonly IUiLog									mLog;
		private readonly ISearchProvider						mSearchProvider;
		private readonly IRandomTrackSelector					mTrackSelector;
		private readonly IPlayCommand							mPlayCommand;
		private readonly IPlayQueue								mPlayQueue;
		private SearchType										mCurrentSearchType;
		private readonly List<SearchType>						mSearchTypes;
		private readonly List<DbTrack>							mPlayList;
		private readonly List<DbTrack>							mApprovalList; 
        private SearchViewNode                                  mSelectedNode;
		private IDisposable										mSearchResultsSubscription;

	    public  IEnumerable<SearchType>                         SearchTypes => mSearchTypes;
		public	ObservableCollectionExtended<SearchViewNode>	SearchResults { get; }

        public SearchViewModel( IEventAggregator eventAggregator, ISearchProvider searchProvider, IRandomTrackSelector trackSelector,
								IPlayCommand playCommand, IPlayQueue playQueue, IUiLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mSearchProvider = searchProvider;
			mTrackSelector = trackSelector;
			mPlayCommand = playCommand;
			mPlayQueue = playQueue;

			mPlayList = new List<DbTrack>();
			mApprovalList = new List<DbTrack>();

			mCurrentSearchType = new SearchType( eSearchItemType.Everything, "Everything" );
			mSearchTypes = new List<SearchType> { mCurrentSearchType,
												  new SearchType( eSearchItemType.Artist, "Artists" ),
												  new SearchType( eSearchItemType.Album, "Albums" ),
												  new SearchType( eSearchItemType.Track, "Tracks" ),
												  new SearchType( eSearchItemType.BandMember, "Band Members" ),
												  new SearchType( eSearchItemType.Biography, "Biographies" ),
//												  new SearchType( eSearchItemType.Discography, "Discographies" ),
												  new SearchType( eSearchItemType.Lyrics, "Lyrics" ),
												  new SearchType( eSearchItemType.SimilarArtist, "Similar Artists" ),
												  new SearchType( eSearchItemType.TopAlbum, "Top Albums" ) };

			SearchResults = new ObservableCollectionExtended<SearchViewNode>();
			mSearchResultsSubscription = mSearchProvider.SearchResults
                .Transform( r => new SearchViewNode( r, OnPlay ))
                .ObserveOnDispatcher()
                .Bind( SearchResults )
                .Subscribe();
		}

		public SearchType CurrentSearchType {
			get => ( mCurrentSearchType );
		    set{ 
				mCurrentSearchType = value;
				RaisePropertyChanged( () => CurrentSearchType );
			}
		}

	    public string SearchText {
			get{ return( Get( () => SearchText )); }
			set{ Set( () => SearchText, value  ); }
		}

		public void Execute_Search() {
			mSearchProvider.StartSearch( CurrentSearchType.ItemType, SearchText );
		}

		public void Execute_PlayRandom() {
			mPlayQueue.Add( mPlayList );

			UpdatePlayList();
		}

		public bool CanExecute_PlayRandom() {
			return( mPlayList.Count > 0 );
		}

        private void UpdatePlayList() {
			mPlayList.Clear();
			mPlayList.AddRange( mTrackSelector.SelectTracks( from searchItem in SearchResults select searchItem.SearchItem, ApproveTrack, cMaxPlayItems ));

			RaiseCanExecuteChangedEvent( "CanExecute_PlayRandom" );
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
            mSearchResultsSubscription?.Dispose();
			mSearchResultsSubscription = null;
        }
    }
}
