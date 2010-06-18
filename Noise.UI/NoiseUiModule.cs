using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Unity;

namespace Noise.UI {
	public class NoiseUiModule : IModule {
		private readonly IUnityContainer    mContainer;

		public NoiseUiModule( IUnityContainer container ) {
			mContainer = container;
		}

		public void Initialize() {
			
		}
	}
}
