using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;
using Observal;
using Observal.Extensions;

namespace Noise.UI.ViewModels {
	public class TrackListViewModel {
		private IUnityContainer		mContainer;
		private IEventAggregator	mEventAggregator;
		private INoiseManager		mNoiseManager;
		private readonly Observer	mChangeObserver;
		private readonly ObservableCollectionEx<TrackViewNode>	mTracks;

		public TrackListViewModel() {
			mTracks = new ObservableCollectionEx<TrackViewNode>();

			mChangeObserver = new Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( node => OnNodeChanged( node.Source ));
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEventAggregator = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				mEventAggregator.GetEvent<Events.ExplorerItemSelected>().Subscribe( OnExplorerItemSelected );
			}
		}

		public ObservableCollection<TrackViewNode> TrackList {
			get{ return( mTracks ); }
		}

		public void OnExplorerItemSelected( object item ) {
			mTracks.Clear();
			mTracks.Each( node => mChangeObserver.Release( node ));

			if( item is DbAlbum ) {
				using( var tracks = mNoiseManager.DataProvider.GetTrackList( item as DbAlbum )) {
					mTracks.AddRange( from track in tracks.List select new TrackViewNode( mEventAggregator, track ));
				}
				mTracks.Each( track => mChangeObserver.Add( track.SettingsNotifier ));
			}
		}

		private void OnNodeChanged( object source ) {
			var notifier = source as UserSettingsNotifier;

			if( notifier != null ) {
				mNoiseManager.DataProvider.UpdateItem( notifier.TargetItem );
			}
		}
	}
}
