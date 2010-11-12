using System.ServiceModel;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;
using Noise.Service.Infrastructure.ServiceContracts;

namespace Noise.Service.Infrastructure.Clients {
	[ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]
	public class LibraryUpdateService : ILibraryUpdateService {
		private readonly IUnityContainer	mContainer;
		private readonly INoiseManager		mNoiseManager;

		public LibraryUpdateService( IUnityContainer container ) {
			mContainer = container;
			mNoiseManager = mContainer.Resolve<INoiseManager>();
		}

		public void StartLibraryUpdate() {
			mNoiseManager.LibraryBuilder.StartLibraryUpdate();
		}

		public void StopLibraryUpdate() {
			mNoiseManager.LibraryBuilder.StopLibraryUpdate();
		}

		public bool IsLibraryUpdateInProgress() {
			return( mNoiseManager.LibraryBuilder.LibraryUpdateInProgress );
		}
	}
}
