using System.Windows;

namespace Noise.Desktop {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
		protected override void OnStartup( StartupEventArgs e ) {
			base.OnStartup( e );

			var bootstrapper = new Bootstrapper();
			bootstrapper.Run();
		}

		protected override void OnExit( ExitEventArgs e ) {
			Desktop.Properties.Settings.Default.Save();

			base.OnExit( e );
		}
	}
}
