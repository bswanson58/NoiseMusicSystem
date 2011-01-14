using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;

namespace Noise.UI.ViewModels {
	public class SimilarSongViewModel : ViewModelBase {
		private const string		cViewStateClosed	= "Closed";
		private const string		cViewStateSearching	= "Searching";
		private const string		cViewStateDisplay	= "Normal";
		private const string		cViewStateNoResults	= "NoResults";

		private const int			cMaxSearchResults = 25;

		private IUnityContainer		mContainer;
		private IEventAggregator	mEvents;
		private	INoiseManager		mNoiseManager;

		private readonly BackgroundWorker						mBackgroundWorker;
		private readonly ObservableCollectionEx<SearchViewNode>	mSearchResults;

		public SimilarSongViewModel() {
			mSearchResults = new ObservableCollectionEx<SearchViewNode>();

			mBackgroundWorker = new BackgroundWorker();
			mBackgroundWorker.DoWork += ( o, args ) => args.Result = BuildSearchList( (long)args.Argument );
			mBackgroundWorker.RunWorkerCompleted += ( o, result ) => SetSearchList( result.Result as IEnumerable<SearchViewNode> );

			VisualStateName = cViewStateClosed;
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEvents = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				mEvents.GetEvent<Events.SimilarSongSearchRequest>().Subscribe( OnSearchRequest );
				mEvents.GetEvent<Events.PlaybackTrackChanged>().Subscribe( OnTrackChanged );
			}
		}

		public string VisualStateName {
			get{ return( Get( () => VisualStateName )); }
			set{ Set( () => VisualStateName, value ); }
		}

		private void OnSearchRequest( long trackId ) {
			VisualStateName = cViewStateSearching;

			if(!mBackgroundWorker.IsBusy ) {
				mBackgroundWorker.RunWorkerAsync( trackId );
			}
		}

		private IEnumerable<SearchViewNode> BuildSearchList( long trackId ) {
			var retValue = new List<SearchViewNode>();
			var	track = mNoiseManager.DataProvider.GetTrack( trackId );

			if( track != null ) {
				retValue.AddRange( from SearchResultItem item
								   in mNoiseManager.SearchProvider.Search( eSearchItemType.Track, string.Format( "\"{0}\"", track.Name ), cMaxSearchResults ) 
								   select new SearchViewNode( item, OnNodeSelected, OnPlay ));

				var ourTrack = retValue.Find( node => node.Track.DbId == trackId );
				if( ourTrack != null ) {
					retValue.Remove( ourTrack );
				}
			}

			return( retValue );
		}

		private void SetSearchList( IEnumerable<SearchViewNode> list ) {
			mSearchResults.SuspendNotification();
			mSearchResults.Clear();
			mSearchResults.AddRange( list );
			mSearchResults.ResumeNotification();

			VisualStateName = mSearchResults.Count() > 0 ? cViewStateDisplay : cViewStateNoResults;
		}

		private void OnTrackChanged( object sender ) {
			Close();
		}

		private void Close() {
			BeginInvoke( () => {
				VisualStateName = cViewStateClosed;

				mSearchResults.Clear();
			});
		}

		private void OnNodeSelected( SearchViewNode node ) {
			if( node.Artist != null ) {
				mEvents.GetEvent<Events.ArtistFocusRequested>().Publish( node.Artist );
			}
			if( node.Album != null ) {
				mEvents.GetEvent<Events.AlbumFocusRequested>().Publish( node.Album );
			}
		}

		private static void OnPlay( SearchViewNode node ) {
			if( node.Track != null ) {
				GlobalCommands.PlayTrack.Execute( node.Track );
			}
			else if( node.Album != null ) {
				GlobalCommands.PlayAlbum.Execute( node.Album );
			}
		}

		public ObservableCollectionEx<SearchViewNode> SearchResults {
			get{ return( mSearchResults ); }
		}

		public void Execute_Close() {
			Close();
		}
	}
}
