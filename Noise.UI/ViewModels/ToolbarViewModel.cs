using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Support;

namespace Noise.UI.ViewModels {
	public class ToolbarViewModel : ViewModelBase {
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
