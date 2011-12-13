using Noise.Infrastructure.Interfaces;

namespace Noise.Core.BackgroundTasks {
	public interface IBackgroundTaskManager {
		bool	Initialize( INoiseManager noiseManager );

		void	Stop();
	}
}
