using System.ServiceModel;
using System.ServiceModel.Web;
using Noise.Infrastructure.RemoteDto;

namespace Noise.Infrastructure.RemoteHost {
	[ServiceContract]
	public interface INoiseRemote {
		[OperationContract]
		[WebGet(ResponseFormat= WebMessageFormat.Json, UriTemplate = "serverVersion")]
		ServerVersion GetServerVersion();

		[OperationContract]
		[WebGet(ResponseFormat= WebMessageFormat.Json, UriTemplate = "serverInformation")]
		RoServerInformation GetServerInformation();

		[OperationContract]
		[WebGet(ResponseFormat= WebMessageFormat.Json, UriTemplate = "setOutputDevice?device={outputDevice}")]
		BaseResult SetOutputDevice( int outputDevice );

		[OperationContract]
		[WebGet(ResponseFormat= WebMessageFormat.Json, UriTemplate = "requestEvents?address={address}")]
		BaseResult RequestEvents( string address );

		[OperationContract]
		[WebGet(ResponseFormat= WebMessageFormat.Json, UriTemplate = "revokeEvents?address={address}")]
		BaseResult RevokeEvents( string address );
	}
}
