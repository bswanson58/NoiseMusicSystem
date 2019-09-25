using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Interfaces;
using Noise.UI.Logging;
using Noise.UI.Models;
using Noise.UI.Support;
using ReusableBits.Interfaces;
using ReusableBits.Mvvm.VersionSpinner;
using ReusableBits.Support;

namespace Noise.UI {
	public class NoiseUiModule : IModule {
		private readonly IUnityContainer    mContainer;

		public NoiseUiModule( IUnityContainer container ) {
			mContainer = container;
		}

		public void Initialize() {
			mContainer.RegisterType<IDialogService, DialogService>();

            mContainer.RegisterType<IPrefixedNameHandler, PrefixedNameHandler>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<ISelectionState, SelectionStateModel>( new ContainerControlledLifetimeManager());
            mContainer.RegisterType<IPlayingItemHandler, PlayingItemHandler>( new TransientLifetimeManager());

			mContainer.RegisterType<IUiLog, UiLogger>( new HierarchicalLifetimeManager());

            mContainer.RegisterType<IVersionFormatter, VersionSpinnerViewModel>( new HierarchicalLifetimeManager());

			var resourceLoader = new ResourceProvider( "Noise.UI", "Resources" );
			mContainer.RegisterInstance<IResourceProvider>( resourceLoader );
		}
	}
}
