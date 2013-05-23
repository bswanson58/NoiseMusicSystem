using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;

namespace Noise.Desktop {
	public class StartupManager : IHandle<Events.NoiseSystemReady>, IHandle<Events.LibraryConfigurationLoaded> {
		private readonly IEventAggregator	mEventAggregator;
		private ILibraryConfiguration		mLibraryConfiguration;
		private	long						mLastLibraryUsed;
		private bool						mLoadLastLibraryOnStartup;

		public StartupManager( IEventAggregator eventAggregator ) {
			mEventAggregator = eventAggregator;

			mEventAggregator.Subscribe( this );
		}

		public void Initialize() {
			mEventAggregator.Publish( new Events.WindowLayoutRequest( Constants.StartupLayout ));

			var expConfig = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if( expConfig != null ) {
				mLastLibraryUsed = expConfig.LastLibraryUsed;
				mLoadLastLibraryOnStartup = expConfig.LoadLastLibraryOnStartup;
			}
		}

		public void Handle( Events.NoiseSystemReady args ) {
			if(( args.WasInitialized ) &&
			   ( mLibraryConfiguration != null )) {
				   if(( mLoadLastLibraryOnStartup ) &&
					  ( mLastLibraryUsed != Constants.cDatabaseNullOid )) {
					   mLibraryConfiguration.Open( mLastLibraryUsed );

					   mEventAggregator.Publish( new Events.WindowLayoutRequest( Constants.ExploreLayout ));
				   }
				   else {
					   mEventAggregator.Publish( mLibraryConfiguration.Libraries.Any() ? new Events.WindowLayoutRequest( Constants.LibrarySelectionLayout ) :
																						 new Events.WindowLayoutRequest( Constants.LibraryCreationLayout ));
				   }
			}
		}

		public void Handle( Events.LibraryConfigurationLoaded args ) {
			mLibraryConfiguration = args.LibraryConfiguration;
		}
	}
}
