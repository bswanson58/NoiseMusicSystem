using System.ServiceModel;
using Noise.Infrastructure.Interfaces;
using Noise.Service.Infrastructure.ServiceContracts;

namespace Noise.Service.Infrastructure.Clients {
	[ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]
	public class LibraryUpdateService : ILibraryUpdateService {
		private readonly ILibraryBuilder	mLibraryBuilder;

		public LibraryUpdateService( ILibraryBuilder libraryBuilder ) {
			mLibraryBuilder = libraryBuilder;
		}

		public void StartLibraryUpdate() {
			mLibraryBuilder.StartLibraryUpdate();
		}

		public void StopLibraryUpdate() {
			mLibraryBuilder.StopLibraryUpdate();
		}

		public bool IsLibraryUpdateInProgress() {
			return( mLibraryBuilder.LibraryUpdateInProgress );
		}
	}
}
