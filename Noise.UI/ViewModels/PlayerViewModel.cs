using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.UI.ViewModels {
	class PlayerViewModel : BindableObject {
		private	readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEvents;
		private readonly INoiseManager		mNoiseManager;
		private DbTrack						mCurrentTrack;
		private StorageFile					mCurrentFile;

		public PlayerViewModel( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();
			mNoiseManager = mContainer.Resolve<INoiseManager>();

			mEvents.GetEvent<Events.TrackSelected>().Subscribe( OnTrackSelected );
		}

		public string CurrentTrackName {
			get {
				var retValue = "None";

				if( mCurrentTrack != null ) {
					retValue = mCurrentTrack.Name;
				}

				return( retValue );
			} 
		}

		public void OnTrackSelected( DbTrack track ) {
			mCurrentTrack = track;
			mCurrentFile = mNoiseManager.DataProvider.GetPhysicalFile( mCurrentTrack );

			NotifyOfPropertyChange( () => CurrentTrackName );
		}
	}
}
