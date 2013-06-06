using System;
using System.Diagnostics;
using System.Reflection;
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

			mBootstrapper = new Bootstrapper();
			mBootstrapper.Run();
		}

		private void CurrentDomainUnhandledException( object sender, UnhandledExceptionEventArgs e ) {
			NoiseLogger.Current.LogException( "Application domain unhandled exception:", e.ExceptionObject as Exception );

			var stackTrace = new StackTrace();
			NoiseLogger.Current.LogMessage( stackTrace.ToString());

			Shutdown( -1 );
		}

		private void AppDispatcherUnhandledException( object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e ) {
			if( Debugger.IsAttached ) {
				Clipboard.SetText( e.Exception.ToString());
			}
	
			NoiseLogger.Current.LogException( "Application unhandled exception:", e.Exception );

			var stackTrace = new StackTrace();
			NoiseLogger.Current.LogMessage( stackTrace.ToString() );

			if( e.Exception is ReflectionTypeLoadException ) {
				var tle = e.Exception as ReflectionTypeLoadException;

				foreach( var ex in tle.LoaderExceptions ) {
					NoiseLogger.Current.LogException( "LoaderException:", ex );
				}
			}

			e.Handled = true;

			Shutdown( -1 );
		}

		private void TaskSchedulerUnobservedTaskException( object sender, UnobservedTaskExceptionEventArgs e ) { 
			NoiseLogger.Current.LogException( "Task Unobserved Exception: ", e.Exception );
 
			e.SetObserved(); 
		} 
	}
}
