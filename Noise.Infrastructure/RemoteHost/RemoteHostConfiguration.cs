using System;

namespace Noise.Infrastructure.RemoteHost {
	public class RemoteHostConfiguration {
		public UInt16	ApiVersion {  get; private set; }
		public UInt16	HostPort {  get; private set; }
		public string	HostName {  get; private set; }

		public RemoteHostConfiguration( UInt16 port, string hostName ) {
			ApiVersion = Constants.cRemoteApiVersion;

			HostPort = port;
			HostName = hostName;
		}
	}
}
