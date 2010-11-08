using System;

namespace Noise.Service.Support {
	public interface IWindowsService : IDisposable {
		void	OnStart( string[] args );
		void	OnStop();
		void	OnPause();
		void	OnContinue();
		void	OnShutdown();
	}
}
