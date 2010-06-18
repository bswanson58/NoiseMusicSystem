﻿using System.Windows;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.UnityExtensions;

namespace Noise.Desktop {
	public class Bootstrapper : UnityBootstrapper {
		protected override DependencyObject CreateShell() {
			var shell = Container.Resolve<Shell>();

			shell.Show();

			return ( shell );
		}

		protected override IModuleCatalog GetModuleCatalog() {
			var catalog = new ModuleCatalog();

			catalog.AddModule( typeof( Core.NoiseCoreModule ))
				.AddModule( typeof( UI.NoiseUiModule ), "NoiseCoreModule" );

			return( catalog );
		}

		protected override void ConfigureContainer() {
			Container.RegisterType<Shell, Shell>();

			base.ConfigureContainer();
		}
	}
}
