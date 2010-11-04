using System;

namespace Noise.Service {
	public interface IWindowsService : IDisposable {
		void	OnStart( string[] args );
		void	OnStop();
		void	OnPause();
		void	OnContinue();
		void	OnShutdown();
	}
}
