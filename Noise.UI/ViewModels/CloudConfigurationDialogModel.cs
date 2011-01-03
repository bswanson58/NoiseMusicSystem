using Noise.Infrastructure;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class CloudConfigurationDialogModel : DialogModelBase {
		public void Execute_Synchronize() {
			GlobalCommands.SynchronizeFromCloud.Execute( this );
		}
	}
}
