using System;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;
using Noise.AppSupport;
using Noise.AppSupport.FeatureToggles;
using Noise.Desktop.Properties;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata;
using Noise.UI.Support;
using Noise.UI.ViewModels;
using ReusableBits.Configuration;

namespace Noise.Desktop {
	public class Bootstrapper : UnityBootstrapper {
		private INoiseManager		mNoiseManager;
		private StartupManager		mStartupManager;
		private WindowManager		mWindowManager;
		private Window				mShell;
		private	ApplicationSupport	mAppSupport;

		public override void Run( bool runWithDefaultConfiguration ) {
			if(!Settings.Default.UpgradePerformed ) {
				try {
					var configUpgrader = new ConfigurationUpgrades();

					ConfigurationUpdater.UpdateConfiguration( Assembly.GetExecutingAssembly(), configUpgrader );

					Settings.Default.Reload();
					Settings.Default.UpgradePerformed = true;
					Settings.Default.Save();
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( ex );

					Settings.Default.Reset();
				}
			}

			base.Run( runWithDefaultConfiguration );
		}

		protected override DependencyObject CreateShell() {
			mShell = Container.Resolve<Shell>();
			mShell.DataContext = this;
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

			mWindowManager = new WindowManager( Container, Container.Resolve<IEventAggregator>());
			mWindowManager.Initialize( mShell );

			var instanceContainer = Container.CreateChildContainer();

			ViewModelResolver.TypeResolver = ( type => instanceContainer.Resolve( type ));
			DialogServiceResolver.Current = instanceContainer.Resolve<IDialogService>();

			mStartupManager = new StartupManager( Container.Resolve<IEventAggregator>());
			mStartupManager.Initialize();

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
		}
	}
}
