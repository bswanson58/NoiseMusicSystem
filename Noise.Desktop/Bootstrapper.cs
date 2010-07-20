using System.Windows;
using Composite.Layout.Configuration;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.UnityExtensions;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Desktop {
	public class Bootstrapper : UnityBootstrapper {
		protected override DependencyObject CreateShell() {
			Execute.InitializeWithDispatcher();

			var shell = Container.Resolve<Shell>();

			shell.Show();

			return ( shell );
		}

		protected override IModuleCatalog GetModuleCatalog() {
			var catalog = new ModuleCatalog();

			catalog.AddModule( typeof( Core.NoiseCoreModule ) )
				.AddModule( typeof( UI.NoiseUiModule ), "NoiseCoreModule" );

			return ( catalog );
		}

		protected override void ConfigureContainer() {
			Container.RegisterType<Shell, Shell>();

			base.ConfigureContainer();
		}
		protected override void InitializeModules() {
			base.InitializeModules();

			StartNoise();
			InitializeLayoutManager();
		}

		private void StartNoise() {
			var	noiseManager = Container.Resolve<INoiseManager>();
			Container.RegisterInstance( noiseManager );
			if( noiseManager.Initialize()) {
//				noiseManager.StartExploring();
			}
		}

		private void InitializeLayoutManager() {
			var layoutManager = LayoutConfigurationManager.LayoutManager;

			layoutManager.Initialize( Container );
			Container.RegisterInstance( layoutManager, new ContainerControlledLifetimeManager());
			//parameterless LoadLayout loads the default Layout into the Shell
			layoutManager.LoadLayout();
		}
	}
}
