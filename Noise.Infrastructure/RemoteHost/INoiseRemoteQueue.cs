using System.ServiceModel;
using System.ServiceModel.Web;
using Noise.Infrastructure.RemoteDto;

namespace Noise.Infrastructure.RemoteHost {
	[ServiceContract]
	public interface INoiseRemoteQueue {
		[OperationContract]
		[WebGet(ResponseFormat= WebMessageFormat.Json, UriTemplate = "enqueueTrack?track={trackId}")]
		BaseResult EnqueueTrack( long trackId );

		[OperationContract]
		[WebGet(ResponseFormat= WebMessageFormat.Json, UriTemplate = "enqueueAlbum?album={albumId}")]
		BaseResult EnqueueAlbum( long albumId );
	}
}
