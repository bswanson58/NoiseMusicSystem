using System.ComponentModel;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.UI.ViewModels {
	public class LibraryAdditionsViewModel {
		private IUnityContainer				mContainer;
		private IEventAggregator			mEvents;
		private INoiseManager				mNoiseManager;
		private readonly BackgroundWorker	mBackgroundWorker;

		public LibraryAdditionsViewModel() {
			
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEvents = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();
			}
		}
	}
}
