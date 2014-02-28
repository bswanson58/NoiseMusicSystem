using System.ServiceModel;
using System.ServiceModel.Web;
using Noise.Infrastructure.RemoteDto;

namespace Noise.Infrastructure.RemoteHost {
	[ServiceContract]
	public interface INoiseRemoteTransport {
		[OperationContract]
		[WebGet(ResponseFormat= WebMessageFormat.Json, UriTemplate = "timeSync?client={clientTimeMilliseconds}")]
		RoTimeSync	SyncClientTime( long clientTimeMilliseconds );
	}
}
