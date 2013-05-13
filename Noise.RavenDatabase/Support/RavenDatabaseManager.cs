using Noise.Infrastructure.Interfaces;

namespace Noise.RavenDatabase.Support {
	public class RavenDatabaseManager : IDatabaseManager {
		public bool Initialize() {
			throw new System.NotImplementedException();
		}

		public void Shutdown() {
			throw new System.NotImplementedException();
		}

		public bool IsOpen { get; private set; }
	}
}
