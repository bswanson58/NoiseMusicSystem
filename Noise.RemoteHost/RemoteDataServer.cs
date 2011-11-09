using Noise.Infrastructure.RemoteHost;

namespace Noise.RemoteHost {
	public class RemoteDataServer : INoiseRemoteData {
		public ServerVersion getServerVersion() {
			return( new ServerVersion { Major = 1, Minor = 0, Build = 0, Revision = 1 });
		}
	}
}
