using System.Collections.Generic;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Resources;
using Noise.UI.Support;
using Noise.UI.ViewModels;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI {
	public class NoiseUiModule : IModule {
		private readonly IUnityContainer    mContainer;

		public NoiseUiModule( IUnityContainer container ) {
			mContainer = container;

			Execute.InitializeWithDispatcher();
		}

		public void Initialize() {
			mContainer.RegisterType<IExplorerViewStrategy, ExplorerStrategyArtistAlbum>( "ArtistAlbum" );
			mContainer.RegisterType<IExplorerViewStrategy, ExplorerStrategyDecade>( "DecadeArtist" );
			mContainer.RegisterType<IEnumerable<IExplorerViewStrategy>, IExplorerViewStrategy[]>();
			mContainer.RegisterType<IResourceProvider, ResourceProvider>();
			mContainer.RegisterType<IDialogService, DialogService>();

			MappingConfiguration.Configure();
		}
	}
}
