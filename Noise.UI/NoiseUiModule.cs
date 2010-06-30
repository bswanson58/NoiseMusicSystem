using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Unity;
using Noise.UI.Support;

namespace Noise.UI {
	public class NoiseUiModule : IModule {
		private readonly IUnityContainer    mContainer;

		public NoiseUiModule( IUnityContainer container ) {
			mContainer = container;

			ViewModelResolver.Container = mContainer;
		}

		public void Initialize() {
		}
	}
}
