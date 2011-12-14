using System.Collections.Generic;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.UI.Support;
using Noise.UI.ViewModels;

namespace Noise.UI {
	public class NoiseUiModule : IModule {
		private readonly IUnityContainer    mContainer;

		public NoiseUiModule( IUnityContainer container ) {
			mContainer = container;
		}

		public void Initialize() {
			mContainer.RegisterType<IExplorerViewStrategy, ExplorerStrategyArtistAlbum>( "ArtistAlbum" );
			mContainer.RegisterType<IExplorerViewStrategy, ExplorerStrategyDecade>( "DecadeArtist" );
			mContainer.RegisterType<IEnumerable<IExplorerViewStrategy>, IExplorerViewStrategy[]>();
			mContainer.RegisterType<IDialogService, DialogService>();

			MappingConfiguration.Configure();
		}
	}
}
