using System.Windows.Threading;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

namespace BundlerUi {
	public class BootstrapperApp : BootstrapperApplication {
		static public Dispatcher BootstrapperDispatcher { get; private set; }

		protected override void Run() {
			Engine.Log( LogLevel.Verbose, "Launching the Noise Music System custom bootstrapper." );
			BootstrapperDispatcher = Dispatcher.CurrentDispatcher;

			var viewModel = new BootstrapperViewModel( this );
			var view = new BootstrapperView{ DataContext = viewModel }; 

			view.Closed += ( sender, e ) => BootstrapperDispatcher.InvokeShutdown();
			view.Show();

			Dispatcher.Run();
			
			Engine.Quit( 0 );
		}
	}
}
