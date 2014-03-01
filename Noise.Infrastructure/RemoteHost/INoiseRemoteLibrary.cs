using System.ServiceModel;
using System.ServiceModel.Web;
using Noise.Infrastructure.RemoteDto;

namespace Noise.Infrastructure.RemoteHost {
	[ServiceContract]
	public interface INoiseRemoteLibrary {
		[OperationContract]
		[WebGet(ResponseFormat= WebMessageFormat.Json, UriTemplate = "selectLibrary?library={libraryId}")]
		BaseResult SelectLibrary( long libraryId );

		[OperationContract]
		[WebGet(ResponseFormat= WebMessageFormat.Json, UriTemplate = "syncLibrary")]
		BaseResult SyncLibrary();

		[OperationContract]
		[WebGet(ResponseFormat= WebMessageFormat.Json, UriTemplate = "libraryList")]
		LibraryListResult GetLibraryList();

		[OperationContract]
		[WebGet(ResponseFormat= WebMessageFormat.Json, UriTemplate = "updateLibrary?library={library}")]
		BaseResult UpdateLibrary( RoLibrary library );

		[OperationContract]
		[WebGet(ResponseFormat= WebMessageFormat.Json, UriTemplate = "createLibrary?library={library}")]
		LibraryListResult CreateLibrary( RoLibrary library );
	}
}
