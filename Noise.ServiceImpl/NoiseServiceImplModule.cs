using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Unity;

namespace Noise.ServiceImpl {
	public class NoiseServiceImplModule : IModule {
		private readonly IUnityContainer    mContainer;

		public NoiseServiceImplModule( IUnityContainer container ) {
			mContainer = container;
		}

		public void Initialize() {
		}
	}
}
