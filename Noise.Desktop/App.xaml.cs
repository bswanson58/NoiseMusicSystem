using System.Windows;
using Noise.Infrastructure;

namespace Noise.Desktop {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
		private INoiseManager	mNoise;

		protected override void OnStartup( StartupEventArgs e ) {
			base.OnStartup( e );

			var bootstrapper = new Bootstrapper();
			bootstrapper.Run();

			mNoise = bootstrapper.Container.Resolve<INoiseManager>();
			mNoise.Initialize();
			mNoise.Explore();
		}
	}
}
