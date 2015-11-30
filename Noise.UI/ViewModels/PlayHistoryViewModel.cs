using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;
using Noise.UI.Logging;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	internal class PlayHistoryViewModel : AutomaticPropertyBase,
										  IHandle<Events.PlayHistoryChanged>, IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private readonly IEventAggregator	mEventAggregator;
		private readonly IUiLog				mLog;
		private readonly IPlayCommand		mPlayCommand;
		private readonly IPlayHistory		mPlayHistory;
		private readonly IArtistProvider	mArtistProvider;
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ITrackProvider		mTrackProvider;
		private TaskHandler					mHistoryRetrievalTask;
		private readonly BindableCollection<PlayHistoryNode>	mHistoryList;

		public PlayHistoryViewModel( IEventAggregator eventAggregator,
									 IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider,
									 IPlayCommand playCommand, IPlayHistory playHistory, IUiLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mPlayCommand = playCommand;
			mPlayHistory = playHistory;

			mHistoryList = new BindableCollection<PlayHistoryNode>();

			mEventAggregator.Subscribe( this );

			BuildHistoryList();
		}

		public void Handle( Events.DatabaseOpened args ) {
			BuildHistoryList();
		}

		public void Handle( Events.DatabaseClosing args ) {
			mHistoryList.Clear();
		}

		public void Handle( Events.PlayHistoryChanged eventArgs ) {
			BuildHistoryList();
		}

		internal TaskHandler HistoryRetrievalTaskHandler {
			get {
				if( mHistoryRetrievalTask == null ) {
					Execute.OnUIThread( () => mHistoryRetrievalTask = new TaskHandler() );
				}

				return ( mHistoryRetrievalTask );
			}
			set { mHistoryRetrievalTask = value; }
		}

		private void BuildHistoryList() {
			HistoryRetrievalTaskHandler.StartTask( () => {
				var historyList = from DbPlayHistory history in mPlayHistory.PlayHistory orderby history.PlayedOn descending select history;

				mHistoryList.Clear();
				mHistoryList.AddRange( from history in historyList
									   let track = mTrackProvider.GetTrack( history.TrackId ) 
									   where track != null let album = mAlbumProvider.GetAlbumForTrack( track ) 
									   where album != null let artist = mArtistProvider.GetArtistForAlbum( album )
									   where artist != null 
									   select new PlayHistoryNode( artist, album, track, history.PlayedOn, OnPlayTrack ));
			},
			() => { },
			ex => mLog.LogException( "Building History List", ex ));
		}

		private void OnPlayTrack( PlayHistoryNode node ) {
			if( node.Track != null ) {
				mPlayCommand.Play( node.Track );
			}
		}

		public BindableCollection<PlayHistoryNode> HistoryList {
			get{ return( mHistoryList ); }
		}

		public PlayHistoryNode SelectedHistory {
			get{ return( Get( () => SelectedHistory )); }
			set {
				Set( () => SelectedHistory, value );

				if( SelectedHistory != null ) {
					if( SelectedHistory.Artist != null ) {
						mEventAggregator.Publish( new Events.ArtistFocusRequested( SelectedHistory.Artist.DbId ));
					}
					if( SelectedHistory.Album != null ) {
						mEventAggregator.Publish( new Events.AlbumFocusRequested( SelectedHistory.Album ));
					}
				}
			}
		}
	}
}
