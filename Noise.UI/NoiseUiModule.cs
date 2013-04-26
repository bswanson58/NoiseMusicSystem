using System.Collections.Generic;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.UI.Support;
using Noise.UI.ViewModels;
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
			mContainer.RegisterType<IExplorerViewStrategy, ExplorerStrategyArtistAlbum>( "ArtistAlbum" );
			mContainer.RegisterType<IExplorerViewStrategy, ExplorerStrategyDecade>( "DecadeArtist" );
			mContainer.RegisterType<IExplorerViewStrategy, ExplorerStrategyGenre>( "GenreArtist" );
			mContainer.RegisterType<IEnumerable<IExplorerViewStrategy>, IExplorerViewStrategy[]>();
			mContainer.RegisterType<IDialogService, DialogService>();

			mContainer.RegisterType<PlaybackFocusTracker, PlaybackFocusTracker>();

			var resourceLoader = new ResourceProvider( "Noise.UI", "Resources" );
			mContainer.RegisterInstance<IResourceProvider>( resourceLoader );

			MappingConfiguration.Configure();
		}
	}
}
