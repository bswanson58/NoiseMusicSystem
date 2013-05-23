using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.Desktop {
	public class StartupManager : IHandle<Events.NoiseSystemReady>, IHandle<Events.LibraryConfigurationLoaded> {
		private readonly IEventAggregator	mEventAggregator;
		private ILibraryConfiguration		mLibraryConfiguration;

		public StartupManager( IEventAggregator eventAggregator ) {
			mEventAggregator = eventAggregator;

			mEventAggregator.Subscribe( this );
		}

		public void Initialize() {
			
		}

		public void Handle( Events.NoiseSystemReady args ) {
			if( args.WasInitialized ) {
				mEventAggregator.Publish( new Events.WindowLayoutRequest( Constants.ExploreLayout ));
			}
		}

		public void Handle( Events.LibraryConfigurationLoaded args ) {
			mLibraryConfiguration = args.LibraryConfiguration;
		}
	}
}
