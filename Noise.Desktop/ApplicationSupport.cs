using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;

namespace Noise.Desktop {
	internal class ApplicationSupport {
		private readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEvents;

		public ApplicationSupport( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();

			mEvents.GetEvent<Events.WebsiteRequest>().Subscribe( OnWebsiteRequested );
		}

		private static void OnWebsiteRequested( string url ) {
			System.Diagnostics.Process.Start( url );
		}
	}
}
