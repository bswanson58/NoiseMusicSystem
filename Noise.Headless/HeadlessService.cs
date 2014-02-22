using System.ServiceProcess;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using ReusableBits.Service;

namespace Noise.Headless {
	[WindowsService("Noise Library Headless Service",
		DisplayName = "Noise Library Headless Service",
		Description = "This service operates the Noise Music System library for remote clients.",
		EventLogSource = "Noise Headless Service",
		StartMode = ServiceStartMode.Automatic)]
	public class HeadlessService : BaseService {
		private readonly INoiseManager		mNoiseManager;

		public HeadlessService( INoiseManager noiseManager ) {
			mNoiseManager = noiseManager;

			var sysConfig = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );
			if((sysConfig != null) &&
			   (sysConfig.EnableRemoteAccess == false )) {
				sysConfig.EnableRemoteAccess = true;

				NoiseSystemConfiguration.Current.Save( sysConfig );
			}
		}

		public override void OnStart( string[] args ) {
			if(!mNoiseManager.Initialize()) {
				NoiseLogger.Current.LogMessage( "Noise Headless Service could not start.");
			}
		}

		public override void OnShutdown() {
			mNoiseManager.Shutdown();
		}
	}
}
