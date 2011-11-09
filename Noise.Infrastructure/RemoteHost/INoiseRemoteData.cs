using System.ServiceModel;
using System.ServiceModel.Web;

namespace Noise.Infrastructure.RemoteHost {
	[ServiceContract]
	public interface INoiseRemoteData {
		[OperationContract]
		[WebGet(ResponseFormat= WebMessageFormat.Json, UriTemplate = "serverVersion")]
		ServerVersion getServerVersion();
	}
}
