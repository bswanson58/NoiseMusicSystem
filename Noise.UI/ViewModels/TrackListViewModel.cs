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
	public class TrackListViewModel : ViewModelBase {
		private IUnityContainer		mContainer;
		private IEventAggregator	mEventAggregator;
		private INoiseManager		mNoiseManager;
		private readonly Observer	mChangeObserver;
		private readonly ObservableCollectionEx<TrackViewNode>	mTracks;

		public TrackListViewModel() {
			mTracks = new ObservableCollectionEx<TrackViewNode>();

			mChangeObserver = new Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnNodeChanged );
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEventAggregator = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				mEventAggregator.GetEvent<Events.DatabaseItemChanged>().Subscribe( OnDatabaseItemChanged );
				mEventAggregator.GetEvent<Events.AlbumFocusRequested>().Subscribe( OnAlbumRequested );
			}
		}

		public ObservableCollection<TrackViewNode> TrackList {
			get{ return( mTracks ); }
		}

		private void OnAlbumRequested( DbAlbum album ) {
			Invoke( () => {
				mTracks.Clear();
				mTracks.Each( node => mChangeObserver.Release( node ));

				using( var tracks = mNoiseManager.DataProvider.GetTrackList( album )) {
					mTracks.AddRange( from track in tracks.List select new TrackViewNode( mEventAggregator, track ));
				}

				mTracks.Each( track => mChangeObserver.Add( track.UiEdit ));
			});
		}

		private void OnDatabaseItemChanged( DbItemChangedArgs args ) {
			if( args.Item is DbTrack ) {
				BeginInvoke( () => {
					var track = ( from TrackViewNode node in mTracks where node.Track.DbId == args.Item.DbId select node ).FirstOrDefault();

					if( track != null ) {
						switch( args.Change ) {
							case DbItemChanged.Update:
								track.UiDisplay.UpdateObject( args.Item );
								break;
							case DbItemChanged.Delete:
								mTracks.Remove( track );

								break;
						}
					}
				});
			}
		}

		private void OnNodeChanged( PropertyChangeNotification propertyNotification ) {
			var notifier = propertyNotification.Source as UserSettingsNotifier;

			if( notifier != null ) {
				var track = notifier.TargetItem as DbTrack;

				if( track != null ) {
					if( propertyNotification.PropertyName == "UiRating" ) {
						mNoiseManager.DataProvider.SetRating( track, notifier.UiRating );
					}
					if( propertyNotification.PropertyName == "UiIsFavorite" ) {
						mNoiseManager.DataProvider.SetFavorite( track, notifier.UiIsFavorite );
					}
				}
			}
		}
	}
}
