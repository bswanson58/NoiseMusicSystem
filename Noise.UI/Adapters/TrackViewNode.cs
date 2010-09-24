using Microsoft.Practices.Composite.Events;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters.DynamicProxies;

namespace Noise.UI.Adapters {
	public class TrackViewNode : ViewModelBase {
		private readonly IEventAggregator	mEventAggregator;
		private bool						mIsSelected;
		public	UpdatingProxy				UiDisplay { get; private set; }
		public	UserSettingsNotifier		UiEdit { get; private set; }
		public	DbTrack						Track { get; private set; }

		public TrackViewNode( IEventAggregator eventAggregator, DbTrack track ) {
			mEventAggregator = eventAggregator;
			UiDisplay = new UpdatingProxy( track );
			UiEdit = new UserSettingsNotifier( track, UiDisplay );
			Track = track;
		}

		public bool IsSelected {
			get { return mIsSelected; }
			set {
				if( value != mIsSelected ) {
					mIsSelected = value;

					RaisePropertyChanged( () => IsSelected );

					if( mIsSelected ) {
						mEventAggregator.GetEvent<Events.TrackSelected>().Publish( Track );
					}
				}
			}
		}

		public void Execute_PlayTrack( object sender ) {
			var track = sender as DbTrack;

			if( track != null ) {
				mEventAggregator.GetEvent<Events.TrackPlayRequested>().Publish( track );
			}
		}
	}
}
