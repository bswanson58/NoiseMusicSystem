using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.UI.Support;
using Noise.UI.Views;

namespace Noise.UI {
	public class NoiseUiModule : IModule {
		private readonly IUnityContainer    mContainer;
		private readonly IRegionManager		mRegionManager;

		public NoiseUiModule( IUnityContainer container ) {
			mContainer = container;
			mRegionManager = mContainer.Resolve<IRegionManager>();

			ViewModelResolver.Container = mContainer;
		}

		public void Initialize() {
			mRegionManager.Regions[RegionNames.ShellRegion].Add( new ShellView());
			mRegionManager.Regions[RegionNames.LibraryExplorerRegion].Add( new LibraryExplorerView());
			mRegionManager.Regions[RegionNames.TrackListRegion].Add( new TrackListView());
			mRegionManager.Regions[RegionNames.PlaybackRegion].Add( new PlayerView());
			mRegionManager.Regions[RegionNames.PlayQueueRegion].Add( new PlayQueueView());
		}
	}
}
