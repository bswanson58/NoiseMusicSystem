using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Noise.Infrastructure;
using ReusableBits.Platform;

namespace Noise.Desktop {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : ISingleInstanceApp {
		private Bootstrapper	mBootstrapper;

		public App() {
            if(!SingleInstance<App>.InitializeAsFirstInstance( Constants.ApplicationName )) {
				Shutdown();
            }
        }

        public bool SignalExternalCommandLineArgs( IList<string> args ) {
            // Bring initial instance to foreground when a second instance is started.
			mBootstrapper.ActivateInstance();

            return true;
        }

        protected override void OnExit( ExitEventArgs e ) {
            // Allow single instance code to perform cleanup operations
            SingleInstance<App>.Cleanup();
        }

        protected override void OnStartup( StartupEventArgs e ) {
			base.OnStartup( e );

			DispatcherUnhandledException += AppDispatcherUnhandledException;
			AppDomain.CurrentDomain.UnhandledException +=CurrentDomainUnhandledException;
			TaskScheduler.UnobservedTaskException += TaskSchedulerUnobservedTaskException;

#if !DEBUG
			var profileDirectory = System.IO.Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), Constants.CompanyName );

			System.Runtime.ProfileOptimization.SetProfileRoot( profileDirectory );
			System.Runtime.ProfileOptimization.StartProfile( "Noise Desktop Startup.Profile" );
#endif

			mBootstrapper = new Bootstrapper();
			mBootstrapper.Run();
		}

		private void CurrentDomainUnhandledException( object sender, UnhandledExceptionEventArgs e ) {
		    mBootstrapper?.LogException( "Application Domain unhandled exception", e.ExceptionObject as Exception );

		    Shutdown( -1 );
		}

		private void AppDispatcherUnhandledException( object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e ) {
			if( Debugger.IsAttached ) {
				Clipboard.SetText( e.Exception.ToString());
			}

		    mBootstrapper?.LogException( "Application Dispatcher unhandled exception", e.Exception );

		    e.Handled = true;
			Shutdown( -1 );
		}

		private void TaskSchedulerUnobservedTaskException( object sender, UnobservedTaskExceptionEventArgs e ) {
		    mBootstrapper?.LogException( "Task Scheduler unobserved exception", e.Exception );

		    e.SetObserved(); 
		}
    }
}
