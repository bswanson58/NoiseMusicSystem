using Microsoft.Practices.Unity;

namespace Noise.UI.ViewModels {
	class TrackListViewModel {
		private readonly IUnityContainer	mContainer;

		public TrackListViewModel( IUnityContainer container ) {
			mContainer = container;
		}
	}
}
