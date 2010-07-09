using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;

namespace Noise.UI.ViewModels {
	public class ConfigurationDialogViewModel {
		private IUnityContainer		mContainer;
		private IEventAggregator	mEvents;

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEvents = mContainer.Resolve<IEventAggregator>();
			}
		}

	}
}
