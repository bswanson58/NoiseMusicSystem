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
			var shellView = new ShellView { DataContext = new ShellViewModel( mContainer ) };

			shellRegion.Add( shellView );

			var explorerRegion = mRegionManager.Regions[RegionNames.LibraryExplorerRegion];
			var explorerView = new LibraryExplorerView { DataContext = new LibraryExplorerViewModel( mContainer ) };

			explorerRegion.Add( explorerView );

			var tracklistRegion = mRegionManager.Regions[RegionNames.TrackListRegion];
			var trackListView = new TrackListView { DataContext = new TrackListViewModel( mContainer ) };

			tracklistRegion.Add( trackListView );
		}
	}
}
