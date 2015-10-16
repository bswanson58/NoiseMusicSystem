using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Noise.Infrastructure;

namespace Noise.Desktop {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App {
		private Bootstrapper	mBootstrapper;

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
			if( mBootstrapper != null ) {
				mBootstrapper.LogException( "Application Domain unhandled exception", e.ExceptionObject as Exception );
			}

			Shutdown( -1 );
		}

		private void AppDispatcherUnhandledException( object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e ) {
			if( Debugger.IsAttached ) {
				Clipboard.SetText( e.Exception.ToString());
			}

			if( mBootstrapper != null ) {
				mBootstrapper.LogException( "Application Dispatcher unhandled exception", e.Exception );
			}

			e.Handled = true;
			Shutdown( -1 );
		}

		private void TaskSchedulerUnobservedTaskException( object sender, UnobservedTaskExceptionEventArgs e ) {
			if( mBootstrapper != null ) {
				mBootstrapper.LogException( "Task Scheduler unobserved exception", e.Exception );
			}
 
			e.SetObserved(); 
		} 
	}
}
