using Caliburn.Micro;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using ReusableBits.Interfaces;
using ReusableBits.Support;

namespace Noise.TenFoot.Ui {
	public class TenFootUiModule : IModule {
		private readonly IUnityContainer    mContainer;

		public TenFootUiModule( IUnityContainer container ) {
			mContainer = container;

			Execute.InitializeWithDispatcher();
		}

		public void Initialize() {
			var resourceLoader = new ResourceProvider( "Noise.TenFoot.Ui", "Resources" );

			mContainer.RegisterInstance<IResourceProvider>( resourceLoader );
		}
	}
}
