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
	}
}
