using System.Windows;
using Noise.Core;

namespace Noise.Desktop {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
		private NoiseManager	mNoise;

		protected override void OnStartup( StartupEventArgs e ) {
			base.OnStartup( e );

			mNoise = new NoiseManager();
			mNoise.Initialize();
		}
	}
}
