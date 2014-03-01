using System.Linq;
using System.ServiceModel;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteDto;
using Noise.Infrastructure.RemoteHost;

namespace Noise.RemoteHost {
	[ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]
	public class RemoteLibraryManager : INoiseRemoteLibrary {
		private readonly ILibraryConfiguration	mLibraryConfiguration;

		public RemoteLibraryManager( ILibraryConfiguration libraryConfiguration ) {
			mLibraryConfiguration = libraryConfiguration;
		}

		public BaseResult SelectLibrary( long libraryId ) {
			var retValue = new BaseResult();
			var	library = mLibraryConfiguration.Libraries.FirstOrDefault( l => l.LibraryId == libraryId );

			if( library != null ) {
				mLibraryConfiguration.Open( library );

				retValue.Success = true;
			}
			else {
				retValue.ErrorMessage = string.Format( "The specified library id ({0}) does not exist", libraryId );
			}

			return( retValue );
		}
	}
}
