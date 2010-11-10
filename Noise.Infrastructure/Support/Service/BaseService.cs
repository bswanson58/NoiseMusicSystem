namespace Noise.Infrastructure.Support.Service {
	public abstract class BaseService : IWindowsService {
		public virtual void Dispose() {
		}

		public virtual void OnStart( string[] args ) {
		}

		public virtual void OnStop() {
		}

		public virtual void OnPause() {
		}

		public virtual void OnContinue() {
		}

		public virtual void OnShutdown() {
		}
	}
}
