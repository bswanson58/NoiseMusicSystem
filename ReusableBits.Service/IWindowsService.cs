using System;

namespace ReusableBits.Service {
	public interface IWindowsService : IDisposable {
		void	OnStart( string[] args );
		void	OnStop();
		void	OnPause();
		void	OnContinue();
		void	OnShutdown();
	}
}
