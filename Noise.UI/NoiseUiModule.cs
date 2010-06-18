using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.UI.ViewModels;
using Noise.UI.Views;

namespace Noise.UI {
	public class NoiseUiModule : IModule {
		private readonly IUnityContainer    mContainer;
		private readonly IRegionManager		mRegionManager;

		public NoiseUiModule( IUnityContainer container ) {
			mContainer = container;
			mRegionManager = mContainer.Resolve<IRegionManager>();
		}

		public void Initialize() {
			var shellRegion = mRegionManager.Regions[RegionNames.ShellRegion];
			var shellView = new ShellView();

			shellView.DataContext = new ShellViewModel();

			shellRegion.Add( shellView );
			shellRegion.Activate( shellView );

			var explorerRegion = mRegionManager.Regions[RegionNames.LibraryExplorerRegion];
			var explorerView = new LibraryExplorerView();

			explorerView.DataContext = new LibraryExplorerViewModel();
			explorerRegion.Add( explorerView );
			explorerRegion.Activate( explorerView );
		}
	}
}
