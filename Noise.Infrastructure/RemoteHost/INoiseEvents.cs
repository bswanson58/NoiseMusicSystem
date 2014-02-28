using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Noise.Infrastructure.RemoteHost {
	[DataContract]
	public class VoidCallbackArgs {
	}

	[ServiceContract]
	public interface INoiseEvents {
        [OperationContract(AsyncPattern = true)]
		[WebGet(ResponseFormat= WebMessageFormat.Json, UriTemplate = "eventInQueue")]
        IAsyncResult BeginEventInQueue( AsyncCallback callback, object state );
        VoidCallbackArgs EndEventInQueue( IAsyncResult result );

        [OperationContract(AsyncPattern = true)]
		[WebGet(ResponseFormat= WebMessageFormat.Json,
			UriTemplate = "eventInTransport?time={serverTime}&state={playState}&track={currentTrack}&position={currentPosition}&length={trackLength}&sequence={sequence}")]
        IAsyncResult BeginEventInTransport( int sequence, int playState, long serverTime, long currentTrack, long currentPosition, long trackLength, AsyncCallback callback, object state );
        VoidCallbackArgs EndEventInTransport( IAsyncResult result );
	}
}
