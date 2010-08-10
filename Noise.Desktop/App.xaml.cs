using System.Windows;

namespace Noise.Desktop {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
		private Bootstrapper	mBootstrapper;

		protected override void OnStartup( StartupEventArgs e ) {
			base.OnStartup( e );

			mBootstrapper = new Bootstrapper();
			mBootstrapper.Run();
		}

		protected override void OnExit( ExitEventArgs e ) {
			Desktop.Properties.Settings.Default.Save();
			mBootstrapper.StopNoise();

			base.OnExit( e );
		}
	}
}
