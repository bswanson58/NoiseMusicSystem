using Composite.Layout;
using Composite.Layout.Configuration;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Desktop.Properties;
using Noise.Infrastructure;
using Noise.UI.Views;

namespace Noise.Desktop {
	internal class WindowManager {
		private readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEvents;
		private readonly ILayoutManager		mLayoutManager;
		private SmallPlayerView				mPlayerView;

		public WindowManager( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();
			mLayoutManager = LayoutConfigurationManager.LayoutManager;

			mEvents.GetEvent<Events.WindowLayoutRequest>().Subscribe( OnWindowLayoutRequested );
		}

		public void Initialize() {
			mLayoutManager.Initialize( mContainer );

			mLayoutManager.LoadLayout( Constants.StreamLayout );
		}

		public void Shutdown() {
			if( mPlayerView != null ) {
				Settings.Default.SmallPlayerTop = mPlayerView.Top;
				Settings.Default.SmallPlayerLeft = mPlayerView.Left;

				mPlayerView.Close();
			}
		}

		public void OnWindowLayoutRequested( string forLayout ) {
			switch( forLayout ) {
				case Constants.SmallPlayerView:
					if( mPlayerView == null ) {
						mPlayerView = new SmallPlayerView { Top = Settings.Default.SmallPlayerTop, Left = Settings.Default.SmallPlayerLeft };

						mPlayerView.Show();
					}
					else {
						mPlayerView.Close();
						mPlayerView = null;
					}
					break;

				default:
					if(( mLayoutManager.CurrentLayout != null ) &&
					   (!string.Equals( mLayoutManager.CurrentLayout.Name, forLayout )) &&
					   ( mLayoutManager.Layouts.Exists( layout => string.Equals( layout.Name, forLayout )))) {
						mLayoutManager.LoadLayout( forLayout );
					}
					break;
			}
		}
	}
}
