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

		public void Execute_LibraryLayout() {
			if( mEvents != null ) {
				mEvents.GetEvent<Events.WindowLayoutRequest>().Publish( Constants.LibraryLayout );
			}
		}

		public void Execute_StreamLayout() {
			if( mEvents != null ) {
				mEvents.GetEvent<Events.WindowLayoutRequest>().Publish( Constants.StreamLayout );
			}
		}
	}
}
