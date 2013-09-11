using System.Windows.Threading;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

namespace BundlerUi {
	public class BootstrapperApp : BootstrapperApplication {
		static public Dispatcher BootstrapperDispatcher { get; private set; }

		protected override void Run() {
			Engine.Log( LogLevel.Verbose, "Launching the Noise Music System custom bootstrapper." );
			BootstrapperDispatcher = Dispatcher.CurrentDispatcher;

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
			
			Engine.Quit( 0 );
		}

		private bool ShouldDisplayUserInterface() {
			return(( Command.Display == Display.Full ) ||
				   ( Command.Display == Display.Unknown ));
		}
	}
}
