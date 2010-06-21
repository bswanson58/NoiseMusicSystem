using Microsoft.Practices.Unity;
using Noise.Infrastructure;

namespace Noise.UI.ViewModels {
	class ShellViewModel {
		private readonly IUnityContainer	mContainer;
		private readonly INoiseManager		mNoiseManager;

		public ShellViewModel( IUnityContainer container ) {
			mContainer = container;
			mNoiseManager = mContainer.Resolve<INoiseManager>();
			
			if( mNoiseManager.Initialize()) {
				mNoiseManager.Explore();
			}
		}
	}
}
