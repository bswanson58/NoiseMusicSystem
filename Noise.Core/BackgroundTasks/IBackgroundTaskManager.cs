namespace Noise.Core.BackgroundTasks {
	public interface IBackgroundTaskManager {
		bool	Initialize();

		void	Stop();
	}
}
