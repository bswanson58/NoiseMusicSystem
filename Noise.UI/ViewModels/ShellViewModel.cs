using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;

namespace Noise.UI.ViewModels {
	public class ShellViewModel {
		private readonly IUnityContainer	mContainer;
		private readonly INoiseManager		mNoiseManager;

		public ShellViewModel( IUnityContainer container ) {
			mContainer = container;
			mNoiseManager = mContainer.Resolve<INoiseManager>();
			mContainer.RegisterInstance<INoiseManager>( mNoiseManager );
			
			if( mNoiseManager.Initialize()) {
//				mNoiseManager.Explore();
			}
		}
	}
}
