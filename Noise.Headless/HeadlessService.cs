using System.ServiceProcess;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using ReusableBits.Service;

namespace Noise.Headless {
	[WindowsService("Noise Library Headless Service",
		DisplayName = "Noise Library Headless Service",
		Description = "This service operates the Noise Music System library for remote clients.",
		EventLogSource = "Noise Headless Service",
		StartMode = ServiceStartMode.Automatic)]
	public class HeadlessService : BaseService {
		private readonly INoiseManager			mNoiseManager;
		private readonly ILibraryConfiguration	mLibraryConfiguration;

		public HeadlessService( INoiseManager noiseManager, ILibraryConfiguration libraryConfiguration ) {
			mNoiseManager = noiseManager;
			mLibraryConfiguration = libraryConfiguration;
		}

		public override void OnStart( string[] args ) {
			NoiseLogger.Current.LogMessage( "===========================" );
			NoiseLogger.Current.LogMessage( "Noise.Headless is starting." );

			if( mNoiseManager.Initialize()) {
				mLibraryConfiguration.OpenDefaultLibrary();
			}
			else {
				NoiseLogger.Current.LogMessage( "Noise Headless Service could not start.");
			}
		}

		public override void OnShutdown() {
			mNoiseManager.Shutdown();
		}
	}
}
