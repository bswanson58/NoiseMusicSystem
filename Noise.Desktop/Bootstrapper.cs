using System.Windows;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.UnityExtensions;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Desktop {
	public class Bootstrapper : UnityBootstrapper {
		private INoiseManager		mNoiseManager;
		private WindowManager		mWindowManager;
		private Window				mShell;
		private	ApplicationSupport	mAppSupport;

		protected override DependencyObject CreateShell() {
			Execute.InitializeWithDispatcher();

			mShell = Container.Resolve<Shell>();
			mShell.Show();
			mShell.Closing += OnShellClosing;

			return ( mShell );
		}

		private void OnShellClosing( object sender, System.ComponentModel.CancelEventArgs e ) {
			StopNoise();
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

			mWindowManager = new WindowManager( Container );
			mWindowManager.Initialize( mShell );
		}

		private void StartNoise() {
			mNoiseManager = Container.Resolve<INoiseManager>();
			Container.RegisterInstance( mNoiseManager );
			mNoiseManager.Initialize();

			mAppSupport = new ApplicationSupport( Container );
		}

		public void StopNoise() {
			if( mNoiseManager != null ) {
				mNoiseManager.Shutdown();

				mNoiseManager = null;
			}
			if( mWindowManager != null ) {
				mWindowManager.Shutdown();

				mWindowManager = null;
			}

			Properties.Settings.Default.Save();
		}
	}
}
