using System.Linq;
using System.ServiceProcess;
using Noise.Infrastructure.Interfaces;
using ReusableBits.Service;

namespace Noise.Headless {
	[WindowsService("Noise Library Headless Service",
		DisplayName = "Noise Library Headless Service",
		Description = "This service operates the Noise Music System library for remote clients.",
		EventLogSource = "Noise Headless Service",
		StartMode = ServiceStartMode.Manual )]
	public class HeadlessService : BaseService {
		private readonly INoiseManager			mNoiseManager;
		private readonly ILibraryConfiguration	mLibraryConfiguration;
		private readonly INoiseLog				mLog;

		public HeadlessService( INoiseManager noiseManager, ILibraryConfiguration libraryConfiguration, INoiseLog log ) {
			mNoiseManager = noiseManager;
			mLibraryConfiguration = libraryConfiguration;
			mLog = log;
		}

		public override void OnStart( string[] args ) {
			mLog.LogMessage( "+++++ Noise.Headless starting. +++++" );

			if( mNoiseManager.Initialize()) {
				mLibraryConfiguration.OpenDefaultLibrary();

				if( mLibraryConfiguration.Current == null ) {
					var library = mLibraryConfiguration.Libraries.FirstOrDefault();

					if( library != null ) {
						mLibraryConfiguration.Open( library );
					}
				}
			}
			else {
				mLog.LogMessage( "Noise Headless Service could not start.");
			}
		}

		public override void OnStop() {
			mNoiseManager.Shutdown();

			mLog.LogMessage( "##### Noise.Desktop System stopped. #####" );
		}
	}
}
