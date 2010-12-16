using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	public class PlayHistoryViewModel : ViewModelBase {
		private IUnityContainer				mContainer;
		private IEventAggregator			mEvents;
		private INoiseManager				mNoiseManager;
		private readonly BackgroundWorker	mBackgroundWorker;
		private readonly ObservableCollectionEx<PlayHistoryNode>	mHistoryList;

		public PlayHistoryViewModel() {
			mHistoryList = new ObservableCollectionEx<PlayHistoryNode>();

			mBackgroundWorker = new BackgroundWorker();
			mBackgroundWorker.DoWork += ( o, args ) => args.Result = BuildHistoryList();
			mBackgroundWorker.RunWorkerCompleted += ( o, result ) => UpdateHistoryList( result.Result as IEnumerable<PlayHistoryNode>);
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEvents = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				mEvents.GetEvent<Events.PlayHistoryChanged>().Subscribe( OnPlayHistoryChanged );

				BackgroundPopulateHistoryList();
			}
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
			BeginInvoke( () => {
				mHistoryList.SuspendNotification();

				mHistoryList.Clear();
				mHistoryList.AddRange( newList );

				mHistoryList.ResumeNotification();
				RaisePropertyChanged( () => HistoryList );
			});
		}

		private IEnumerable<PlayHistoryNode> BuildHistoryList() {
			var retValue = new List<PlayHistoryNode>();
			var historyList = from DbPlayHistory history in mNoiseManager.PlayHistory.PlayHistory orderby history.PlayedOn descending select history;

			foreach( var history in historyList ) {
				var track = mNoiseManager.DataProvider.GetTrack( history.TrackId );

				if( track != null ) {
					var album = mNoiseManager.DataProvider.GetAlbumForTrack( track );

					if( album != null ) {
						var artist = mNoiseManager.DataProvider.GetArtistForAlbum( album );

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
				mEvents.GetEvent<Events.ArtistFocusRequested>().Publish( node.Artist );
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
