using System.Windows;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;
using Noise.AppSupport;
using Noise.AudioSupport;
using Noise.Desktop.Properties;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata;
using Noise.UI.Support;
using Noise.UI.ViewModels;

namespace Noise.Desktop {
	public class Bootstrapper : UnityBootstrapper {
		private INoiseManager		mNoiseManager;
		private StartupManager		mStartupManager;
		private WindowManager		mWindowManager;
		private Window				mShell;
		private	ApplicationSupport	mAppSupport;
		private INoiseLog			mLog;

		protected override DependencyObject CreateShell() {
			mShell = Container.Resolve<Shell>();
			mShell.DataContext = new DisabledWindowCommandsViewModel();
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
				.AddModule( typeof( UI.NoiseUiModule ), "NoiseCoreModule" )
				.AddModule( typeof( AudioSupportModule ))
				.AddModule( typeof( BlobStorage.BlobStorageModule ))
				.AddModule( typeof( NoiseMetadataModule ))
				.AddModule( typeof( RemoteHost.RemoteHostModule ))
				.AddModule( typeof( EntityFrameworkDatabase.EntityFrameworkDatabaseModule ));

			return ( catalog );
		}

		protected override void ConfigureContainer() {
			Container.RegisterType<Shell, Shell>();

			base.ConfigureContainer();

			var iocConfig = new IocConfiguration( Container );
			iocConfig.InitializeIoc( ApplicationUsage.Desktop );

			mLog = Container.Resolve<INoiseLog>();
		}

		protected override void InitializeModules() {
			base.InitializeModules();

			mWindowManager = new WindowManager( Container, Container.Resolve<IEventAggregator>(), Container.Resolve<IPreferences>());
			mWindowManager.Initialize( mShell );

			var instanceContainer = Container.CreateChildContainer();

			ViewModelResolver.TypeResolver = ( type => instanceContainer.Resolve( type ));
			DialogServiceResolver.Current = instanceContainer.Resolve<IDialogService>();

			mStartupManager = Container.Resolve<StartupManager>();
			mStartupManager.Initialize();

			mLog.LogMessage( "+++ Noise.Desktop System starting. +++" );

			StartNoise( instanceContainer );
		}

		private async void StartNoise( IUnityContainer container ) {
			mNoiseManager = container.Resolve<INoiseManager>();
			mAppSupport = container.Resolve<ApplicationSupport>();

			await mNoiseManager.AsyncInitialize();
			mAppSupport.Initialize();

			mShell.DataContext = container.Resolve<WindowCommandsViewModel>();
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

			Settings.Default.Save();

			mLog.LogMessage( "### Noise.Desktop System stopped. ###" );
		}
	}
}
