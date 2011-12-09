using System;
using System.Diagnostics;
using System.Reflection;
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

			DispatcherUnhandledException += App_DispatcherUnhandledException;
			AppDomain.CurrentDomain.UnhandledException +=CurrentDomain_UnhandledException;

			mBootstrapper = new Bootstrapper();
			mBootstrapper.Run();
		}

		private void CurrentDomain_UnhandledException( object sender, UnhandledExceptionEventArgs e ) {
			NoiseLogger.Current.LogException( "Application domain unhandled exception:", e.ExceptionObject as Exception );

			Shutdown( -1 );
		}

		private void App_DispatcherUnhandledException( object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e ) {
			if( Debugger.IsAttached ) {
				Clipboard.SetText( e.Exception.ToString());
			}
	
			NoiseLogger.Current.LogException( "Application unhandled exception:", e.Exception );

			if( e.Exception is ReflectionTypeLoadException ) {
				var tle = e.Exception as ReflectionTypeLoadException;

				foreach( var ex in tle.LoaderExceptions ) {
					NoiseLogger.Current.LogException( "LoaderException:", ex );
				}
			}
			e.Handled = true;

			Shutdown( -1 );
		}
	}
}
