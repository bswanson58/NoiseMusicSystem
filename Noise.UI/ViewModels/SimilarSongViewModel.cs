using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Events;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class SimilarSongViewModel : ViewModelBase,
										IHandle<Events.PlaybackTrackChanged>, IHandle<Events.SimilarSongSearchRequest> {
		private const string		cViewStateClosed	= "Closed";
		private const string		cViewStateSearching	= "Searching";
		private const string		cViewStateDisplay	= "Normal";
		private const string		cViewStateNoResults	= "NoResults";

		private const int			cMaxSearchResults = 25;

		private readonly IEventAggregator	mEvents;
		private readonly ICaliburnEventAggregator	mEventAggregator;
		private readonly ITrackProvider		mTrackProvider;
		private readonly ISearchProvider	mSearchProvider;
		private readonly BackgroundWorker	mBackgroundWorker;
		private readonly ObservableCollectionEx<SearchViewNode>	mSearchResults;

		public SimilarSongViewModel( IEventAggregator eventAggregator, ICaliburnEventAggregator caliburnEventAggregator,
									 ITrackProvider trackProvider, ISearchProvider searchProvider ) {
			mEvents = eventAggregator;
			mEventAggregator = caliburnEventAggregator;
			mTrackProvider = trackProvider;
			mSearchProvider = searchProvider;

			mEventAggregator.Subscribe( this );
			mEvents.GetEvent<Events.BalloonPopupOpened>().Subscribe( OnPopupOpened );

			mSearchResults = new ObservableCollectionEx<SearchViewNode>();

			mBackgroundWorker = new BackgroundWorker();
			mBackgroundWorker.DoWork += ( o, args ) => args.Result = BuildSearchList( (long)args.Argument );
			mBackgroundWorker.RunWorkerCompleted += ( o, result ) => SetSearchList( result.Result as IEnumerable<SearchViewNode> );

			VisualStateName = cViewStateClosed;
		}

		public string VisualStateName {
			get{ return( Get( () => VisualStateName )); }
			set {
				Set( () => VisualStateName, value );

				if( value != cViewStateClosed ) {
					mEvents.GetEvent<Events.BalloonPopupOpened>().Publish( this );
				}
			}
		}

		public void Handle( Events.SimilarSongSearchRequest eventArgs ) {
			VisualStateName = cViewStateSearching;

			if(!mBackgroundWorker.IsBusy ) {
				mBackgroundWorker.RunWorkerAsync( eventArgs.TrackId );
			}
		}

		private IEnumerable<SearchViewNode> BuildSearchList( long trackId ) {
			var retValue = new List<SearchViewNode>();
			var	track = mTrackProvider.GetTrack( trackId );

			if( track != null ) {
				retValue.AddRange( from SearchResultItem item
								   in mSearchProvider.Search( eSearchItemType.Track, string.Format( "\"{0}\"", track.Name ), cMaxSearchResults ) 
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

			VisualStateName = mSearchResults.Any() ? cViewStateDisplay : cViewStateNoResults;
		}

		public void Handle( Events.PlaybackTrackChanged eventArgs ) {
			Close();
		}

		private void OnPopupOpened( object sender ) {
			if( sender != this ) {
				Close();
			}
		}

		private void Close() {
			Execute.OnUIThread( () => {
				VisualStateName = cViewStateClosed;

				mSearchResults.Clear();
			});
		}

		private void OnNodeSelected( SearchViewNode node ) {
			if( node.Artist != null ) {
				mEventAggregator.Publish( new Events.ArtistFocusRequested( node.Artist.DbId ));
			}
			if( node.Album != null ) {
				mEventAggregator.Publish( new Events.AlbumFocusRequested( node.Album ));
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
