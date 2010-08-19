using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Support;
using Noise.UI.Views;

namespace Noise.UI.ViewModels {
	public class ToolbarViewModel : ViewModelBase {
		private IUnityContainer		mContainer;
		private IEventAggregator	mEvents;

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				if( mContainer != null ) {
					mEvents = mContainer.Resolve<IEventAggregator>();
				}
			}
		}

		public void Execute_Configuration() {
			var	dialog = new ConfigurationDialog();

			dialog.ShowDialog();
		}

		public void Execute_SmallPlayerView() {
			if( mEvents != null ) {
				mEvents.GetEvent<Events.WindowLayoutRequest>().Publish( Constants.SmallPlayerView );
			}
		}

		public void Execute_OpenUrl() {
//			var noiseManager = mContainer.Resolve<INoiseManager>();
//			var stream = new DbInternetStream { Url = "http://provisioning.streamtheworld.com/pls/WXRTFMAAC.pls", Encoding = eAudioEncoding.AAC,
//												IsPlaylistWrapped = true, Name = "WXRT" };

//			noiseManager.AudioPlayer.OpenStream( stream );
		}
	}
}
