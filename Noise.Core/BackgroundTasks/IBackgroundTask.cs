using Noise.Infrastructure.Interfaces;

namespace Noise.Core.BackgroundTasks {
	public interface IBackgroundTask {
		string		TaskId { get; }

		bool		Initialize( INoiseManager noiseMgr );

		void		ExecuteTask();
		void		Shutdown();
	}
}
