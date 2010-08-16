using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Views;

namespace Noise.UI.ViewModels {
	public class ToolbarViewModel : ViewModelBase {
		private IUnityContainer		mContainer;
		private SmallPlayerView		mPlayerView;

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set { mContainer = value; }
		}

		public void Execute_Configuration() {
			var	dialog = new ConfigurationDialog();

			dialog.ShowDialog();
		}

		public void Execute_SmallPlayerView() {
			if( mPlayerView == null ) {
				mPlayerView = new SmallPlayerView();
				mPlayerView.Show();
			}
			else {
				mPlayerView.Close();
				mPlayerView = null;
			}
		}

		public void Execute_OpenUrl() {
			var noiseManager = mContainer.Resolve<INoiseManager>();

			noiseManager.AudioPlayer.OpenStream( "http://provisioning.streamtheworld.com/pls/WXRTFMAAC.pls" );
		}
	}
}
