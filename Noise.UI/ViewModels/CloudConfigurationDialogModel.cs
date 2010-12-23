using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class CloudConfigurationDialogModel : DialogModelBase {
		private readonly INoiseManager	mNoiseManager;

		public CloudConfigurationDialogModel( INoiseManager noiseManager ) {
			mNoiseManager = noiseManager;	
		}

		public void Execute_Synchronize() {
			mNoiseManager.CloudSyncMgr.CreateSynchronization();
		}
	}
}
