using System;

namespace Noise.Infrastructure.Support.Service {
	public interface IWindowsService : IDisposable {
		void	OnStart( string[] args );
		void	OnStop();
		void	OnPause();
		void	OnContinue();
		void	OnShutdown();
	}
}
