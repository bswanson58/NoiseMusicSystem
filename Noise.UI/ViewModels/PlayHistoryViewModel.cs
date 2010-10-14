using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;

namespace Noise.UI.ViewModels {
	public class PlayHistoryViewModel : ViewModelBase {
		private IUnityContainer			mContainer;
		private IEventAggregator		mEvents;
		private INoiseManager			mNoiseManager;
		private readonly ObservableCollectionEx<PlayHistoryNode>	mHistoryList;

		public PlayHistoryViewModel() {
			mHistoryList = new ObservableCollectionEx<PlayHistoryNode>();
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEvents = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				mEvents.GetEvent<Events.PlayHistoryChanged>().Subscribe( OnPlayHistoryChanged );

				PopulatePlayHistory();
			}
		}

		private void OnPlayHistoryChanged( IPlayHistory playHistory ) {
			PopulatePlayHistory();
		}

		private void PopulatePlayHistory() {
			BeginInvoke( () => {
				mHistoryList.SuspendNotification();
				mHistoryList.Clear();

				var historyList = from DbPlayHistory history in mNoiseManager.PlayHistory.PlayHistory orderby history.PlayedOn descending select history;
				foreach( var history in historyList ) {
					var track = mNoiseManager.DataProvider.GetTrack( history.Track );

					if( track != null ) {
						var album = mNoiseManager.DataProvider.GetAlbumForTrack( track );

						if( album != null ) {
							var artist = mNoiseManager.DataProvider.GetArtistForAlbum( album );

							if( artist != null ) {
								mHistoryList.Add( new PlayHistoryNode( artist, album, track, history.PlayedOn, OnNodeSelected, OnPlayTrack ));
							}
						}
					}
				}

				mHistoryList.ResumeNotification();
				RaisePropertyChanged( () => HistoryList );
			});
		}

		private void OnNodeSelected( PlayHistoryNode node ) {
			if( node.Artist != null ) {
				mEvents.GetEvent<Events.ArtistFocusRequested>().Publish( node.Artist );
			}
			if( node.Album != null ) {
				mEvents.GetEvent<Events.AlbumFocusRequested>().Publish( node.Album );
			}
		}

		private void OnPlayTrack( PlayHistoryNode node ) {
			if( node.Track != null ) {
				mEvents.GetEvent<Events.TrackPlayRequested>().Publish( node.Track );
			}
		}

		public ObservableCollection<PlayHistoryNode> HistoryList {
			get{ return( mHistoryList ); }
		}
	}
}
