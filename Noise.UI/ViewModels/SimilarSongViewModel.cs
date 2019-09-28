﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;

namespace Noise.UI.ViewModels {
	public class SimilarSongViewModel : ViewModelBase,
										IHandle<Events.PlaybackTrackChanged>, IHandle<Events.SimilarSongSearchRequest>, IHandle<Events.BalloonPopupOpened> {
		private const string		cViewStateClosed	= "Closed";
		private const string		cViewStateSearching	= "Searching";
		private const string		cViewStateDisplay	= "Normal";
		private const string		cViewStateNoResults	= "NoResults";

		private const int			cMaxSearchResults = 25;

		private readonly IEventAggregator	mEventAggregator;
		private readonly ITrackProvider		mTrackProvider;
		private readonly ISearchProvider	mSearchProvider;
		private readonly IPlayCommand		mPlayCommand;
		private readonly BackgroundWorker	mBackgroundWorker;
		private readonly ObservableCollectionEx<SearchViewNode>	mSearchResults;
        private SearchViewNode              mSelectedNode;

	    public  ObservableCollectionEx<SearchViewNode> SearchResults => mSearchResults;

	    public SimilarSongViewModel( IEventAggregator eventAggregator, IPlayCommand playCommand,
									 ITrackProvider trackProvider, ISearchProvider searchProvider ) {
			mEventAggregator = eventAggregator;
			mTrackProvider = trackProvider;
			mSearchProvider = searchProvider;
			mPlayCommand = playCommand;

			mEventAggregator.Subscribe( this );

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
					mEventAggregator.PublishOnUIThread( new Events.BalloonPopupOpened( ViewNames.SimilarSongView ));
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
								   select new SearchViewNode( item, OnPlay ));

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

		public void Handle( Events.BalloonPopupOpened eventArgs ) {
			if(!eventArgs.ViewName.Equals( ViewNames.SimilarSongView )) {
				Close();
			}
		}

		private void Close() {
			Execute.OnUIThread( () => {
				VisualStateName = cViewStateClosed;

				mSearchResults.Clear();
			});
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

	    public void Execute_Close() {
			Close();
		}
	}
}
