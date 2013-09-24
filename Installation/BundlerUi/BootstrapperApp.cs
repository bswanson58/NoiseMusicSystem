using System;
using System.Windows.Threading;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

namespace BundlerUi {
	public class BootstrapperApp : BootstrapperApplication {
		static public Dispatcher BootstrapperDispatcher { get; private set; }

		protected override void Run() {
			Engine.Log( LogLevel.Verbose, "(NMS):Launching the Noise Music System custom bootstrapper." );

			AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
			BootstrapperDispatcher = Dispatcher.CurrentDispatcher;

			try {
				var viewModel = new BootstrapperViewModel( this ) { InvokeShutdownOnComplete = !ShouldDisplayUserInterface() };

				viewModel.Initialize();
				viewModel.StartDetect();

				if( ShouldDisplayUserInterface()) {
					var view = new BootstrapperView { DataContext = viewModel };

					view.Closed += ( sender, e ) => viewModel.StartShutdown();
					view.Show();
				}
				else {
					if( Command.Action == LaunchAction.Uninstall ) {
						viewModel.StartUninstall();
					}
				}

				Dispatcher.Run();
			}
			catch( Exception ex ) {
				Engine.Log( LogLevel.Error, string.Format( "(NMS):BootstrapperApp:Run Exception ({0}): {1}\r\nStack trace:\r\n{2}", ex.GetType(), ex.Message, ex.StackTrace ));
			}
			
			Engine.Quit( 0 );
		}

		private bool ShouldDisplayUserInterface() {
			return(( Command.Display == Display.Full ) ||
				   ( Command.Display == Display.Unknown ));
		}

		private void CurrentDomainUnhandledException( object sender, UnhandledExceptionEventArgs e ) {
			Engine.Log( LogLevel.Error, string.Format( "(NMS):Application domain unhandled exception: {0}", e.ExceptionObject as Exception ));

			Engine.Quit( -1 );
			Dispatcher.ExitAllFrames();
		}
	}
}
