using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	public class PlayHistoryViewModel : ViewModelBase {
		private readonly IEventAggregator	mEvents;
		private readonly ICaliburnEventAggregator	mEventAggregator;
		private readonly IPlayHistory		mPlayHistory;
		private readonly IArtistProvider	mArtistProvider;
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ITrackProvider		mTrackProvider;
		private readonly BackgroundWorker	mBackgroundWorker;
		private readonly ObservableCollectionEx<PlayHistoryNode>	mHistoryList;

		public PlayHistoryViewModel( IEventAggregator eventAggregator, ICaliburnEventAggregator caliburnEventAggregator,
									 IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, IPlayHistory playHistory ) {
			mEvents = eventAggregator;
			mEventAggregator = caliburnEventAggregator;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mPlayHistory = playHistory;

			mHistoryList = new ObservableCollectionEx<PlayHistoryNode>();

			mEvents.GetEvent<Events.PlayHistoryChanged>().Subscribe( OnPlayHistoryChanged );

			mBackgroundWorker = new BackgroundWorker();
			mBackgroundWorker.DoWork += ( o, args ) => args.Result = BuildHistoryList();
			mBackgroundWorker.RunWorkerCompleted += ( o, result ) => UpdateHistoryList( result.Result as IEnumerable<PlayHistoryNode>);

			BackgroundPopulateHistoryList();
		}

		private void OnPlayHistoryChanged( IPlayHistory playHistory ) {
			BackgroundPopulateHistoryList();
		}

		private void BackgroundPopulateHistoryList() {
			if(!mBackgroundWorker.IsBusy ) {
				mBackgroundWorker.RunWorkerAsync();
			}
		}

		private void UpdateHistoryList( IEnumerable<PlayHistoryNode> newList ) {
			Execute.OnUIThread( () => {
				mHistoryList.SuspendNotification();

				mHistoryList.Clear();
				mHistoryList.AddRange( newList );

				mHistoryList.ResumeNotification();
				RaisePropertyChanged( () => HistoryList );
			});
		}

		private IEnumerable<PlayHistoryNode> BuildHistoryList() {
			var retValue = new List<PlayHistoryNode>();
			var historyList = from DbPlayHistory history in mPlayHistory.PlayHistory orderby history.PlayedOn descending select history;

			foreach( var history in historyList ) {
				var track = mTrackProvider.GetTrack( history.TrackId );

				if( track != null ) {
					var album = mAlbumProvider.GetAlbumForTrack( track );

					if( album != null ) {
						var artist = mArtistProvider.GetArtistForAlbum( album );

						if( artist != null ) {
							retValue.Add( new PlayHistoryNode( artist, album, track, history.PlayedOn, OnNodeSelected, OnPlayTrack ));
						}
					}
				}
			}

			return( retValue );
		}

		private void OnNodeSelected( PlayHistoryNode node ) {
			if( node.Artist != null ) {
				mEventAggregator.Publish( new Events.ArtistFocusRequested( node.Artist.DbId ));
			}
			if( node.Album != null ) {
				mEvents.GetEvent<Events.AlbumFocusRequested>().Publish( node.Album );
			}
		}

		private static void OnPlayTrack( PlayHistoryNode node ) {
			if( node.Track != null ) {
				GlobalCommands.PlayTrack.Execute( node.Track );
			}
		}

		public ObservableCollection<PlayHistoryNode> HistoryList {
			get{ return( mHistoryList ); }
		}
	}
}
