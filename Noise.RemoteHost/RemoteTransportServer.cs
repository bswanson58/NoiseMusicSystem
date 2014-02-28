using System;
using System.ServiceModel;
using Noise.Infrastructure.RemoteDto;
using Noise.Infrastructure.RemoteHost;

namespace Noise.RemoteHost {
	[ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]
	public class RemoteTransportServer : INoiseRemoteTransport {

		public RoTimeSync SyncClientTime( long clientTimeMilliseconds ) {
			return( new RoTimeSync( clientTimeMilliseconds, CurrentMillisecond ));
		}

	    private static readonly DateTime JanuaryFirst1970 = new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );
		private static long CurrentMillisecond {
			get { return((long)(( DateTime.UtcNow - JanuaryFirst1970 ).TotalMilliseconds )); }
		}
	}
}
