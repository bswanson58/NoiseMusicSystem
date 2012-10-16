using System.Windows;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;
using Noise.AppSupport;
using Noise.AppSupport.FeatureToggles;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata;
using Noise.UI.Support;

namespace Noise.Desktop {
	public class Bootstrapper : UnityBootstrapper {
		private INoiseManager		mNoiseManager;
		private WindowManager		mWindowManager;
		private Window				mShell;
		private	ApplicationSupport	mAppSupport;

		protected override DependencyObject CreateShell() {
			mShell = Container.Resolve<Shell>();
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
				.AddModule( typeof( BlobStorage.BlobStorageModule ))
				.AddModule( typeof( NoiseMetadataModule ))
				.AddModule( typeof( RemoteHost.RemoteHostModule ));

			catalog.AddModule(( new EntityDatabaseEnabled()).FeatureEnabled ? typeof( EntityFrameworkDatabase.EntityFrameworkDatabaseModule )
																			: typeof( EloqueraDatabase.EloqueraDatabaseModule ));

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

			mWindowManager = new WindowManager( Container, Container.Resolve<IEventAggregator>());
			mWindowManager.Initialize( mShell );
		}

		private void StartNoise() {
			var instanceContainer = Container.CreateChildContainer();

			mNoiseManager = instanceContainer.Resolve<INoiseManager>();
			mAppSupport = instanceContainer.Resolve<ApplicationSupport>();

			mNoiseManager.Initialize();
			mAppSupport.Initialize();

			ViewModelResolver.TypeResolver = ( type => instanceContainer.Resolve( type));
			DialogServiceResolver.Current = instanceContainer.Resolve<IDialogService>();
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
