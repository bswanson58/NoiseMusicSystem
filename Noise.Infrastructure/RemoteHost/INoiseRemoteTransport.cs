﻿using System.ServiceModel;
using System.ServiceModel.Web;
using Noise.Infrastructure.RemoteDto;

namespace Noise.Infrastructure.RemoteHost {
	[ServiceContract]
	public interface INoiseRemoteTransport {
		[OperationContract]
		[WebGet(ResponseFormat= WebMessageFormat.Json, UriTemplate = "timeSync?client={clientTimeMilliseconds}")]
		RoTimeSync	SyncClientTime( long clientTimeMilliseconds );

		[OperationContract]
		[WebGet(ResponseFormat= WebMessageFormat.Json, UriTemplate = "getTransportState")]
		RoTransportState GetTransportState();

		[OperationContract]
		[WebGet(ResponseFormat= WebMessageFormat.Json, UriTemplate = "getAudioState")]
		AudioStateResult GetAudioState();

		[OperationContract]
		[WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat= WebMessageFormat.Json, Method = "PUT", UriTemplate = "setAudioState")]
		BaseResult SetAudioState( RoAudioState audioState );
	}
}
