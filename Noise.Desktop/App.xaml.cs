using System;
using System.Windows;
using Noise.Infrastructure.Interfaces;

namespace Noise.Desktop {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
		private Bootstrapper	mBootstrapper;

		protected override void OnStartup( StartupEventArgs e ) {
			base.OnStartup( e );

			DispatcherUnhandledException += App_DispatcherUnhandledException;
			AppDomain.CurrentDomain.UnhandledException +=CurrentDomain_UnhandledException;

			mBootstrapper = new Bootstrapper();
			mBootstrapper.Run();
		}

		private void CurrentDomain_UnhandledException( object sender, UnhandledExceptionEventArgs e ) {
			var	log = mBootstrapper.Container.Resolve<ILog>();

			log.LogException( "Application domain unhandled exception:", e.ExceptionObject as Exception );

			Shutdown( -1 );
		}

		private void App_DispatcherUnhandledException( object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e ) {
			var	log = mBootstrapper.Container.Resolve<ILog>();

			log.LogException( "Application unhandled exception:", e.Exception );
			e.Handled = true;

			Shutdown( -1 );
		}

		protected override void OnExit( ExitEventArgs e ) {
			Desktop.Properties.Settings.Default.Save();
			mBootstrapper.StopNoise();

			base.OnExit( e );
		}
	}
}
