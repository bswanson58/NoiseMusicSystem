using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;

namespace Noise.Service.LibraryService {
	public class LibraryServiceImpl : IWindowsService {
		private readonly IUnityContainer	mContainer;
		private INoiseManager				mNoiseManager;

		public LibraryServiceImpl( IUnityContainer container ) {
			mContainer = container;
		}

		public void OnStart( string[] args ) {
			mNoiseManager = mContainer.Resolve<INoiseManager>();
			mContainer.RegisterInstance( mNoiseManager );

			mNoiseManager.Initialize();
		}

		public void OnStop() {
			mNoiseManager.Shutdown();
		}

		public void OnPause() {

		}

		public void OnContinue() {

		}

		public void OnShutdown() {

		}

		public void Dispose() {

		}
	}
}
