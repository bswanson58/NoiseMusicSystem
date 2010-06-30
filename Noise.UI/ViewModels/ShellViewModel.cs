using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;

namespace Noise.UI.ViewModels {
	public class ShellViewModel {
		private IUnityContainer		mContainer;
		private INoiseManager		mNoiseManager;

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mNoiseManager = mContainer.Resolve<INoiseManager>();
				mContainer.RegisterInstance( mNoiseManager );
				if( mNoiseManager.Initialize()) {
//					mNoiseManager.Explore();
				}
			}
		}
	}
}
