using System.Windows;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;
using Noise.AppSupport;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Support;

namespace Noise.Desktop {
	public class Bootstrapper : UnityBootstrapper {
		private INoiseManager		mNoiseManager;
		private WindowManager		mWindowManager;
		private Window				mShell;
		private	ApplicationSupport	mAppSupport;

		protected override DependencyObject CreateShell() {
			Execute.InitializeWithDispatcher();

			mShell = Container.Resolve<Shell>();
			mShell.DataContext = Container.Resolve<ShellViewModel>();
			mShell.Show();
			mShell.Closing += OnShellClosing;

#if( DEBUG )
			BindingErrorListener.Listen( message => MessageBox.Show( message ));
#endif

			return ( mShell );
		}

		private void OnShellClosing( object sender, System.ComponentModel.CancelEventArgs e ) {
			StopNoise();
		}

		protected override IModuleCatalog CreateModuleCatalog() {
			var catalog = new ModuleCatalog();

			catalog.AddModule( typeof( Core.NoiseCoreModule ))
				.AddModule( typeof( Service.Infrastructure.ServiceInfrastructureModule ))
				.AddModule( typeof( UI.NoiseUiModule ), "NoiseCoreModule" )
				.AddModule( typeof( EloqueraDatabase.EloqueraDatabaseModule ))
				.AddModule( typeof( RemoteHost.RemoteHostModule ));

			return ( catalog );
		}

		protected override void ConfigureContainer() {
			Container.RegisterType<Shell, Shell>();

			base.ConfigureContainer();

			var iocConfig = new IocConfiguration( Container );
			iocConfig.InitializeIoc( ApplicationUsage.Desktop );
		}

		protected override void InitializeModules() {
			base.InitializeModules();

			StartNoise();

			mWindowManager = new WindowManager( Container );
			mWindowManager.Initialize( mShell );
		}

		private void StartNoise() {
			mAppSupport = new ApplicationSupport( Container );

			mNoiseManager = Container.Resolve<INoiseManager>();
			Container.RegisterInstance<INoiseManager>( mNoiseManager );
			mNoiseManager.Initialize();
			mNoiseManager.StartExplorerJobs();
			mAppSupport.Initialize();
		}

		public void StopNoise() {
			mAppSupport.Shutdown();

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
