using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;

namespace Noise.Desktop {
	public class StartupManager : IHandle<Events.NoiseSystemReady>, IHandle<Events.LibraryConfigurationLoaded> {
		private readonly IEventAggregator	mEventAggregator;
		private readonly IPreferences		mPreferences;
		private ILibraryConfiguration		mLibraryConfiguration;
		private	long						mLastLibraryUsed;
		private bool						mLoadLastLibraryOnStartup;

		public StartupManager( IEventAggregator eventAggregator, IPreferences preferences ) {
			mEventAggregator = eventAggregator;
			mPreferences = preferences;

			mEventAggregator.Subscribe( this );
		}

		public void Initialize() {
			var preferences = mPreferences.Load<NoiseCorePreferences>();

			mLastLibraryUsed = preferences.LastLibraryUsed;
			mLoadLastLibraryOnStartup = preferences.LoadLastLibraryOnStartup;

			mEventAggregator.PublishOnUIThread( new Events.WindowLayoutRequest( Constants.StartupLayout ));
		}

		public void Handle( Events.NoiseSystemReady args ) {
			if(( args.WasInitialized ) &&
			   ( mLibraryConfiguration != null )) {
				   if(( mLoadLastLibraryOnStartup ) &&
					  ( mLastLibraryUsed != Constants.cDatabaseNullOid )) {
					   mLibraryConfiguration.Open( mLastLibraryUsed );

					   mEventAggregator.PublishOnUIThread( new Events.WindowLayoutRequest( Constants.ExploreLayout ));
				   }
				   else {
					   mEventAggregator.PublishOnUIThread( mLibraryConfiguration.Libraries.Any() ? new Events.WindowLayoutRequest( Constants.LibrarySelectionLayout ) :
																						 new Events.WindowLayoutRequest( Constants.LibraryCreationLayout ));
				   }
			}
		}

		public void Handle( Events.LibraryConfigurationLoaded args ) {
			mLibraryConfiguration = args.LibraryConfiguration;
		}
	}
}
