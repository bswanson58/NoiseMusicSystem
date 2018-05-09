using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;
using Noise.UI.Logging;
using ReusableBits;

namespace Noise.UI.ViewModels {
	public class SearchType {
		public eSearchItemType	ItemType { get; private set; }
		public string			DisplayName { get; private set; }

		public SearchType( eSearchItemType itemType, string displayName ) {
			ItemType = itemType;
			DisplayName = displayName;
		}
	}

	internal class SearchViewModel : ViewModelBase {
		private const int										cMaxSearchResults = 100;
		private const int										cMaxPlayItems = 10;

		private readonly IEventAggregator						mEventAggregator;
		private readonly IUiLog									mLog;
		private readonly ISearchProvider						mSearchProvider;
		private readonly IRandomTrackSelector					mTrackSelector;
		private readonly IPlayCommand							mPlayCommand;
		private readonly IPlayQueue								mPlayQueue;
		private SearchType										mCurrentSearchType;
		private TaskHandler<IEnumerable<SearchResultItem>>		mSearchTask; 
		private readonly List<SearchType>						mSearchTypes;
		private readonly List<DbTrack>							mPlayList;
		private readonly List<DbTrack>							mApprovalList; 
		private readonly ObservableCollectionEx<SearchViewNode>	mSearchResults;

		public SearchViewModel( IEventAggregator eventAggregator, ISearchProvider searchProvider, IRandomTrackSelector trackSelector,
								IPlayCommand playCommand, IPlayQueue playQueue, IUiLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mSearchProvider = searchProvider;
			mTrackSelector = trackSelector;
			mPlayCommand = playCommand;
			mPlayQueue = playQueue;

			mSearchResults = new ObservableCollectionEx<SearchViewNode>();
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
		}

		public SearchType CurrentSearchType {
			get{ return( mCurrentSearchType ); }
			set{ 
				mCurrentSearchType = value;
				RaisePropertyChanged( () => CurrentSearchType );
			}
		}

		public IEnumerable<SearchType> SearchTypes {
			get{ return( mSearchTypes ); }
		}

		public string SearchText {
			get{ return( Get( () => SearchText )); }
			set{ Set( () => SearchText, value  ); }
		}

		public void Execute_Search() {
			BuildSearchList();
		}

		public void Execute_PlayRandom() {
			mPlayQueue.Add( mPlayList );

			UpdatePlayList();
		}

		public bool CanExecute_PlayRandom() {
			return( mPlayList.Count > 0 );
		}

		protected TaskHandler<IEnumerable<SearchResultItem>> SearchTask {
			get {
				if( mSearchTask == null ) {
					Execute.OnUIThread( () => mSearchTask = new TaskHandler<IEnumerable<SearchResultItem>>());
				}

				return( mSearchTask );
			}
			set {  mSearchTask = value; }
		} 

		private void BuildSearchList() {
			SearchTask.StartTask( () => mSearchProvider.Search( CurrentSearchType.ItemType, SearchText, cMaxSearchResults ),
								  UpdateSearchList,
								  error => mLog.LogException( string.Format( "Building Search List for \"{0}\"", SearchText ), error ));
		}

		private void UpdateSearchList( IEnumerable<SearchResultItem> searchList ) {
			mSearchResults.Clear();
			mApprovalList.Clear();

			if( searchList != null ) {
				var searchResultItems = searchList.ToList();

				mSearchResults.SuspendNotification();
				mSearchResults.AddRange( from searchItem in searchResultItems select new SearchViewNode( searchItem, OnNodeSelected, OnPlay ));
				mSearchResults.ResumeNotification();
			}

			UpdatePlayList();
		}

		private void UpdatePlayList() {
			mPlayList.Clear();
			mPlayList.AddRange( mTrackSelector.SelectTracks( from searchItem in mSearchResults select searchItem.SearchItem, ApproveTrack, cMaxPlayItems ));

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

		private void OnNodeSelected( SearchViewNode node ) {
			if( node.Artist != null ) {
				mEventAggregator.PublishOnUIThread( new Events.ArtistFocusRequested( node.Artist.DbId ));
			}
			if( node.Album != null ) {
				mEventAggregator.PublishOnUIThread( new Events.AlbumFocusRequested( node.Album ));
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

		public ObservableCollectionEx<SearchViewNode> SearchResults {
			get{ return( mSearchResults ); }
		}
	}
}
