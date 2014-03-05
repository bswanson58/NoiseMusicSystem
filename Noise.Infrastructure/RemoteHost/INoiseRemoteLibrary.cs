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
		[WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat= WebMessageFormat.Json, Method = "PUT", UriTemplate = "updateLibrary")]
		BaseResult UpdateLibrary( RoLibrary library );

		[OperationContract]
		[WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat= WebMessageFormat.Json, Method="POST", UriTemplate = "createLibrary")]
		LibraryListResult CreateLibrary( RoLibrary library );
	}
}
