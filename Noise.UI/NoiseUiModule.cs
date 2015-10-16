using Caliburn.Micro;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.UI.Interfaces;
using Noise.UI.Logging;
using Noise.UI.Models;
using Noise.UI.Support;
using ReusableBits.Interfaces;
using ReusableBits.Support;

namespace Noise.UI {
	public class NoiseUiModule : IModule {
		private readonly IUnityContainer    mContainer;

		public NoiseUiModule( IUnityContainer container ) {
			mContainer = container;

			Execute.InitializeWithDispatcher();
		}

		public void Initialize() {
			mContainer.RegisterType<IDialogService, DialogService>();

			mContainer.RegisterType<ISelectionState, SelectionStateModel>( new ContainerControlledLifetimeManager());

			mContainer.RegisterType<IUiLog, UiLogger>( new HierarchicalLifetimeManager());

			var resourceLoader = new ResourceProvider( "Noise.UI", "Resources" );
			mContainer.RegisterInstance<IResourceProvider>( resourceLoader );

			MappingConfiguration.Configure();
		}
	}
}
