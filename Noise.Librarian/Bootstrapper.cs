using System;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;
using Noise.AppSupport;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.Librarian.ViewModels;
using Noise.Librarian.Views;
using Noise.UI.Support;

namespace Noise.Librarian {
	public class Bootstrapper : UnityBootstrapper {
		private Window				mShell;
		private IEventAggregator	mEventAggregator;
		private ILifecycleManager	mLifecycleManager;
		private INoiseLog			mLog;

		protected override IModuleCatalog CreateModuleCatalog() {
			var catalog = new ModuleCatalog();

			catalog.AddModule( typeof( Core.NoiseCoreModule ))
				.AddModule( typeof( UI.NoiseUiModule ), "NoiseCoreModule" )
				.AddModule( typeof( BlobStorage.BlobStorageModule ))
				.AddModule( typeof( Metadata.NoiseMetadataModule ))
				.AddModule( typeof( EntityFrameworkDatabase.EntityFrameworkDatabaseModule ));

			return ( catalog );
		}

		protected override void ConfigureContainer() {
            // Caliburn Micro dispatcher initialize.
            PlatformProvider.Current = new XamlPlatformProvider();

			Container.RegisterType<Shell, Shell>();

            base.ConfigureContainer();

			var iocConfig = new IocConfiguration( Container );
			iocConfig.InitializeIoc( ApplicationUsage.Librarian );

			mLog = Container.Resolve<INoiseLog>();

			ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver( ViewModelTypeResolver );
			ViewModelLocationProvider.SetDefaultViewModelFactory( CreateViewModel );

			Container.Resolve<IModuleManager>().Run();
		}

		protected override DependencyObject CreateShell() {
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
			Shutdown();
		}

		protected override void InitializeModules() {
			base.InitializeModules();

			Startup();
		}

		private void Startup() {
			mLog.LogMessage( "+++++ Noise.Librarian starting. +++++" );

            try {
				mEventAggregator = Container.Resolve<IEventAggregator>();
				mLifecycleManager = Container.Resolve<ILifecycleManager>();

                mLifecycleManager.Initialize();

                mLog.LogMessage( "Initialized LibrarianModel." );
                mEventAggregator.PublishOnUIThread( new Events.SystemInitialized());
            }
            catch( Exception ex ) {
                mLog.LogException( "Failed to Initialize", ex );
            }
		}

        private void Shutdown() {
            mEventAggregator.PublishOnUIThread( new Events.SystemShutdown());

            mLifecycleManager.Shutdown();
//            mDatabaseManager.Shutdown();

            mLog.LogMessage( "Shutdown LibrarianModel." );
        }

		private Type ViewModelTypeResolver( Type viewType ) {
			var viewModelName = viewType.Name.Replace( "View", "ViewModel" );
			var viewModelType = GetType().Assembly.GetTypes().FirstOrDefault( type => type.Name == viewModelName );

			return( viewModelType );
		}

		private object CreateViewModel( Type modelType ) {
			return( Container.Resolve( modelType ));
		}

		public void LogException( string reason, Exception exception ) {
            mLog?.LogException( reason, exception );
        }
	}
}
