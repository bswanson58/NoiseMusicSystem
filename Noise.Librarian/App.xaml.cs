using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Noise.AppSupport;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.Librarian.ViewModels;
using Noise.Librarian.Views;
using Noise.UI.Support;
using Prism.Ioc;
using Prism.Modularity;

namespace Noise.Librarian {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App {
        private Window				mShell;
        private IEventAggregator	mEventAggregator;
        private ILifecycleManager	mLifecycleManager;
        private INoiseLog			mLog;

		protected override void OnStartup( StartupEventArgs e ) {
			base.OnStartup( e );

			DispatcherUnhandledException += AppDispatcherUnhandledException;
			AppDomain.CurrentDomain.UnhandledException +=CurrentDomainUnhandledException;
			TaskScheduler.UnobservedTaskException += TaskSchedulerUnobservedTaskException;
		}

        protected override void RegisterTypes( IContainerRegistry containerRegistry ) {
            // Caliburn Micro dispatcher initialize.
            PlatformProvider.Current = new XamlPlatformProvider();

			RegisterNoiseModules();

            var iocConfig = Container.Resolve<IocConfiguration>();
            iocConfig.InitializeIoc( ApplicationUsage.Librarian );

            mLog = Container.Resolve<INoiseLog>();

            Container.Resolve<IModuleManager>().Run();
        }

        protected override Window CreateShell() {
            mShell = Container.Resolve<Shell>();
            mShell.DataContext = Container.Resolve<ShellViewModel>();
            mShell.Show();
            mShell.Closing += OnShellClosing;

#if( DEBUG )
            BindingErrorListener.Listen( message => MessageBox.Show( message ));
#endif
//            Startup();

            return ( mShell );
        }

        private void OnShellClosing( object sender, System.ComponentModel.CancelEventArgs e ) {
            StopLibrarian();
            Shutdown();
        }

		protected override void InitializeModules() {
			base.InitializeModules();

            InitializeNoiseModules();
            StartLibrarian();
		}

        private void RegisterNoiseModules() {
            RegisterModule( typeof( Core.NoiseCoreModule ));
            RegisterModule( typeof( BlobStorage.BlobStorageModule ));
            RegisterModule( typeof( Metadata.NoiseMetadataModule ));
            RegisterModule( typeof( EntityFrameworkDatabase.EntityFrameworkDatabaseModule ));
            RegisterModule( typeof( UI.NoiseUiModule ));
        }

        private void RegisterModule( Type moduleType ) {
            if( Container.Resolve( moduleType ) is IModule module ) {
                module.RegisterTypes( Container as IContainerRegistry);
            }
        }

        private void InitializeNoiseModules() {
            InitializeModule( typeof( Core.NoiseCoreModule ));
            InitializeModule( typeof( BlobStorage.BlobStorageModule ));
            InitializeModule( typeof( Metadata.NoiseMetadataModule ));
            InitializeModule( typeof( EntityFrameworkDatabase.EntityFrameworkDatabaseModule ));
            InitializeModule( typeof( UI.NoiseUiModule ));
        }

        private void InitializeModule( Type moduleType ) {
            if( Container.Resolve( moduleType ) is IModule module ) {
                module.OnInitialized( Container );
            }
        }

        private void StartLibrarian() {
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

        private void StopLibrarian() {
            mEventAggregator.PublishOnUIThread( new Events.SystemShutdown());

            mLifecycleManager.Shutdown();
//            mDatabaseManager.Shutdown();

            mLog.LogMessage( "Shutdown LibrarianModel." );
        }

		public void LogException( string reason, Exception exception ) {
            mLog?.LogException( reason, exception );
        }

        private void CurrentDomainUnhandledException( object sender, UnhandledExceptionEventArgs e ) {
            mLog.LogException( "Application Domain unhandled exception", e.ExceptionObject as Exception );

			Shutdown( -1 );
		}

		private void AppDispatcherUnhandledException( object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e ) {
			if( Debugger.IsAttached ) {
				Clipboard.SetText( e.Exception.ToString());
			}

            mLog.LogException( "Application Dispatcher unhandled exception", e.Exception );

			e.Handled = true;
			Shutdown( -1 );
		}

		private void TaskSchedulerUnobservedTaskException( object sender, UnobservedTaskExceptionEventArgs e ) {
            mLog.LogException( "Task Scheduler unobserved exception", e.Exception );
 
			e.SetObserved(); 
		} 
	}
}
