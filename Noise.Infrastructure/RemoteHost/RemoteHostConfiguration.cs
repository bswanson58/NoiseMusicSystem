using System;

namespace Noise.Infrastructure.RemoteHost {
	public class RemoteHostConfiguration {
		public UInt16	ApiVersion {  get; private set; }
		public UInt16	HostPort {  get; private set; }
		public string	ServerName {  get; private set; }

		public RemoteHostConfiguration( UInt16 port, string serverName ) {
			ApiVersion = Constants.cRemoteApiVersion;

			HostPort = port;
			ServerName = serverName;
		}
	}
}
