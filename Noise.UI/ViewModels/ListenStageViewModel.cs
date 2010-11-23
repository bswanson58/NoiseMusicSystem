using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.UI.ViewModels {
	public class ListenStageViewModel : ViewModelBase {
		private	IUnityContainer		mContainer;
		private IEventAggregator	mEvents;
		private INoiseManager		mNoiseManager;

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mNoiseManager = mContainer.Resolve<INoiseManager>();
				mEvents = mContainer.Resolve<IEventAggregator>();
				mEvents.GetEvent<Events.PlaybackTrackStarted>().Subscribe( OnTrackStarted );
			}
		}

		private void OnTrackStarted( PlayQueueTrack track ) {
			
		}
	}
}
