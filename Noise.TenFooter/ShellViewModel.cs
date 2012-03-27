using Caliburn.Micro;
using Noise.Infrastructure.Interfaces;

namespace Noise.TenFooter {
    public class ShellViewModel : Conductor<object>.Collection.OneActive, IShell {
		private readonly INoiseManager	mNoiseManager;

		public ShellViewModel( INoiseManager noiseManager ) {
			mNoiseManager = noiseManager;
		}

		protected override void OnActivate() {
		}
    }
}
