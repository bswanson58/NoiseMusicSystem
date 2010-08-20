using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Unity;
using Noise.UI.Support;
using Noise.UI.ViewModels;

namespace Noise.UI {
	public class NoiseUiModule : IModule {
		private readonly IUnityContainer    mContainer;

		public NoiseUiModule( IUnityContainer container ) {
			mContainer = container;

			ViewModelResolver.Container = mContainer;
		}

		public void Initialize() {
			mContainer.RegisterType<IExplorerViewStrategy, ExplorerStrategyArtistAlbum>( "ArtistAlbum" );
			mContainer.RegisterType<IDialogService, DialogService>();
		}
	}
}
